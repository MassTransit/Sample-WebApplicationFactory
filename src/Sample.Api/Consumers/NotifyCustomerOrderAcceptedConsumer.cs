using MassTransit;
using Sample.Contracts;

namespace Sample.Api.Consumers;

public class NotifyCustomerOrderAcceptedConsumer :
    IConsumer<OrderAccepted>
{
    readonly ILogger<NotifyCustomerOrderAcceptedConsumer> _logger;

    public NotifyCustomerOrderAcceptedConsumer(ILogger<NotifyCustomerOrderAcceptedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderAccepted> context)
    {
        _logger.LogInformation("Order Accepted: {OrderId}", context.Message.OrderId);
        return Task.CompletedTask;
    }
}