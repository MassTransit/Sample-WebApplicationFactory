using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sample.Api.StateMachines;
using Sample.Contracts;
using Xunit;
using Xunit.Abstractions;

namespace Sample.XUnitTests;

public class When_request_sent_from_state_machine
{
    public When_request_sent_from_state_machine(ITestOutputHelper outputHelper)
    {
        OutputHelper = new TestOutputHelperTextWriterAdapter(outputHelper);
    }

    TestOutputHelperTextWriterAdapter OutputHelper { get; }

    [Fact]
    public async Task Should_use_correlation_id_for_request_id()
    {
        await using var provider = new ServiceCollection()
            .AddTelemetryListener(OutputHelper)
            .AddMassTransitTestHarness(OutputHelper, x =>
            {
                x.AddHandler<ValidateOrder>(context => context.RespondAsync(new OrderValidated(context.Message.OrderId)));

                x.AddSagaStateMachine<OrderStateMachine, OrderState, OrderStateDefinition>();
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

            Assert.True(await sagaHarness.Consumed.Any<OrderValidated>());

            Assert.True(await harness.Published.Any<OrderAccepted>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}