using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Sample.Api.StateMachines;
using Sample.Contracts;

namespace Sample.Tests;

[TestFixture]
public class When_request_sent_from_state_machine
{
    [Test]
    public async Task Should_use_correlation_id_for_request_id()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddHandler<ValidateOrder>(context => context.RespondAsync(new OrderValidated(context.Message.OrderId)));

                x.AddSagaStateMachine<OrderStateMachine, OrderState, OrderStateDefinition>()
                    .RedisRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetTestHarness();

        await harness.Start();

        var client = harness.GetRequestClient<SubmitOrder>();

        var orderId = Guid.NewGuid();

        try
        {
            await client.GetResponse<OrderSubmissionAccepted>(new SubmitOrder(orderId));

            var sagaHarness = harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();

            Assert.That(await sagaHarness.Consumed.Any<OrderValidated>(), Is.True);

            Assert.That(await harness.Published.Any<OrderAccepted>(), Is.True);
        }
        finally
        {
            await harness.Stop();
        }
    }
}