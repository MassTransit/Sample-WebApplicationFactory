using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Sample.Api;
using Sample.Api.StateMachines;
using Sample.Contracts;
using Xunit;
using Xunit.Abstractions;

namespace Sample.XUnitTests;

public class Submitting_an_order
{
    public Submitting_an_order(ITestOutputHelper outputHelper)
    {
        OutputHelper = new TestOutputHelperTextWriterAdapter(outputHelper);
    }

    TestOutputHelperTextWriterAdapter OutputHelper { get; }

    [Fact]
    public async Task Should_have_the_submitted_status()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            {
                services.AddTelemetryListener(OutputHelper);
                services.AddMassTransitTestHarness(OutputHelper);
            }));

        var testHarness = application.Services.GetTestHarness();

        using var client = application.CreateClient();

        const string submitOrderUrl = "/Order";

        var orderId = NewId.NextGuid();

        var submitOrderResponse = await client.PostAsync(submitOrderUrl, JsonContent.Create(new Order
        {
            OrderId = orderId
        }));

        submitOrderResponse.EnsureSuccessStatusCode();
        var orderStatus = await submitOrderResponse.Content.ReadFromJsonAsync<OrderStatus>();

        Assert.NotNull(orderStatus);
        Assert.Equal(orderStatus.OrderId, orderId);

        var sagaTestHarness = testHarness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();

        Assert.True(await sagaTestHarness.Consumed.Any<SubmitOrder>(x => x.Context.Message.OrderId == orderId));

        var sagaExists = await sagaTestHarness.Exists(orderId, x => x.Submitted);
        Assert.True(sagaExists.HasValue);
        Assert.Equal(orderId, sagaExists.Value);

        var getOrderStatusUrl = $"/Order/{orderId:D}";

        var orderStatusResponse = await client.GetAsync(getOrderStatusUrl);
        orderStatusResponse.EnsureSuccessStatusCode();

        orderStatus = await orderStatusResponse.Content.ReadFromJsonAsync<OrderStatus>();

        Assert.NotNull(orderStatus);
        Assert.Equal(orderId, orderStatus.OrderId);
        Assert.Equal(nameof(OrderStateMachine.Submitted), orderStatus.Status);
    }

    [Fact]
    public async Task Should_have_the_validated_status()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            {
                services.AddTelemetryListener(OutputHelper);
                services.AddMassTransitTestHarness(OutputHelper,
                    x => { x.AddHandler<ValidateOrder>(context => context.RespondAsync(new OrderValidated(context.Message.OrderId))); });
            }));

        var testHarness = application.Services.GetTestHarness();

        using var client = application.CreateClient();

        const string submitOrderUrl = "/Order";

        var orderId = NewId.NextGuid();

        var submitOrderResponse = await client.PostAsync(submitOrderUrl, JsonContent.Create(new Order
        {
            OrderId = orderId
        }));

        submitOrderResponse.EnsureSuccessStatusCode();
        var orderStatus = await submitOrderResponse.Content.ReadFromJsonAsync<OrderStatus>();

        Assert.NotNull(orderStatus);
        Assert.Equal(orderId, orderStatus.OrderId);

        var sagaTestHarness = testHarness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();

        Assert.True(await sagaTestHarness.Consumed.Any<SubmitOrder>(x => x.Context.Message.OrderId == orderId));

        var sagaExists = await sagaTestHarness.Exists(orderId, x => x.Accepted);
        Assert.True(sagaExists.HasValue);
        Assert.Equal(orderId, sagaExists.Value);

        var getOrderStatusUrl = $"/Order/{orderId:D}";

        var orderStatusResponse = await client.GetAsync(getOrderStatusUrl);
        orderStatusResponse.EnsureSuccessStatusCode();

        orderStatus = await orderStatusResponse.Content.ReadFromJsonAsync<OrderStatus>();

        Assert.NotNull(orderStatus);
        Assert.Equal(orderId, orderStatus.OrderId);
        Assert.Equal(nameof(OrderStateMachine.Accepted), orderStatus.Status);
    }

    [Fact]
    public async Task Should_pass_the_test()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            {
                services.AddMassTransitTestHarness(x =>
                {
                    x.AddHandler<ValidateOrder>(context => context.RespondAsync(new OrderValidated(context.Message.OrderId)));
                });
            }));

        var testHarness = application.Services.GetTestHarness();

        var @event = new OrderSubmitted(Guid.NewGuid());

        await testHarness.Bus.Publish(@event);

        var published = await testHarness.Published.Any<OrderSubmitted>();
        var consumed = await testHarness.Consumed.Any<OrderSubmitted>();
        var faults = await testHarness.Published.Any<Fault<OrderSubmitted>>();

        Assert.True(published);
        Assert.True(consumed);
        Assert.False(faults);
    }
}