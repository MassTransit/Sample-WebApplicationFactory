using System.Net.Http.Json;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Sample.Api;
using Sample.Api.StateMachines;
using Sample.Contracts;
using Xunit;
using Xunit.Abstractions;

namespace Sample.XUnitTests;

public class WithoutHarnessSpecs
{
    public WithoutHarnessSpecs(ITestOutputHelper outputHelper)
    {
        OutputHelper = new TestOutputHelperTextWriterAdapter(outputHelper);
    }

    TestOutputHelperTextWriterAdapter OutputHelper { get; }

    [Fact]
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

        Assert.NotNull(orderStatus);
        Assert.Equal(orderId, orderStatus.OrderId);

        var getOrderStatusUrl = $"/Order/{orderId:D}";

        var orderStatusResponse = await client.GetAsync(getOrderStatusUrl);
        orderStatusResponse.EnsureSuccessStatusCode();

        orderStatus = await orderStatusResponse.Content.ReadFromJsonAsync<OrderStatus>();

        Assert.NotNull(orderStatus);
        Assert.Equal(orderId, orderStatus.OrderId);
        Assert.Equal(nameof(OrderStateMachine.Submitted), orderStatus.Status);
    }
}