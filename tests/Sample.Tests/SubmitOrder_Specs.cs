using System.Net.Http.Json;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Sample.Api;
using Sample.Api.Contracts;
using Sample.Api.StateMachines;

namespace Sample.Tests;

public class Submitting_an_order
{
    [Test]
    public async Task Should_have_the_submitted_status()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.ConfigureServices(services => services.AddMassTransitTestHarness()));

        var testHarness = application.Services.GetTestHarness();

        using var client = application.CreateClient();

        var sagaTestHarness = testHarness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();

        const string submitOrderUrl = "/Order";

        var orderId = NewId.NextGuid();

        var submitOrderResponse = await client.PostAsync(submitOrderUrl, JsonContent.Create(new Order
        {
            OrderId = orderId
        }));

        submitOrderResponse.EnsureSuccessStatusCode();
        var orderStatus = await submitOrderResponse.Content.ReadFromJsonAsync<OrderStatus>();

        Assert.That(orderStatus, Is.Not.Null);
        Assert.That(orderStatus!.OrderId, Is.EqualTo(orderId));

        Assert.That(await sagaTestHarness.Consumed.Any<SubmitOrder>(x => x.Context.Message.OrderId == orderId), Is.True);

        var sagaExists = await sagaTestHarness.Exists(orderId, x => x.Submitted);
        Assert.That(sagaExists.HasValue);
        Assert.That(sagaExists!.Value, Is.EqualTo(orderId));

        var getOrderStatusUrl = $"/Order?id={orderId:D}";

        var orderStatusResponse = await client.GetAsync(getOrderStatusUrl);
        orderStatusResponse.EnsureSuccessStatusCode();

        orderStatus = await orderStatusResponse.Content.ReadFromJsonAsync<OrderStatus>();

        Assert.That(orderStatus, Is.Not.Null);
        Assert.That(orderStatus!.OrderId, Is.EqualTo(orderId));
        Assert.That(orderStatus.Status, Is.EqualTo(nameof(OrderStateMachine.Submitted)));
    }
}