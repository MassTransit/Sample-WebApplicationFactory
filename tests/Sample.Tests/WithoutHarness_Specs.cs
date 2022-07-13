using System.Net.Http.Json;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Sample.Api;
using Sample.Api.StateMachines;
using Sample.Contracts;

namespace Sample.Tests;

public class WithoutHarness_Specs
{
    [Test]
    public async Task Should_have_the_submitted_status()
    {
        await using var application = new WebApplicationFactory<Program>();

        using var client = application.CreateClient();

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

        var getOrderStatusUrl = $"/Order/{orderId:D}";

        var orderStatusResponse = await client.GetAsync(getOrderStatusUrl);
        orderStatusResponse.EnsureSuccessStatusCode();

        orderStatus = await orderStatusResponse.Content.ReadFromJsonAsync<OrderStatus>();

        Assert.That(orderStatus, Is.Not.Null);
        Assert.That(orderStatus!.OrderId, Is.EqualTo(orderId));
        Assert.That(orderStatus.Status, Is.EqualTo(nameof(OrderStateMachine.Submitted)));
    }
}