using MassTransit;
using Sample.Contracts;

namespace Sample.Api.Consumers;

public class NotifyCustomerOrderSubmittedConsumer :
    IConsumer<OrderSubmitted>
{
    readonly ILogger<NotifyCustomerOrderSubmittedConsumer> _logger;

    public NotifyCustomerOrderSubmittedConsumer(ILogger<NotifyCustomerOrderSubmittedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        _logger.LogInformation("Order Submitted: {OrderId}", context.Message.OrderId);
        return Task.CompletedTask;
    }
}