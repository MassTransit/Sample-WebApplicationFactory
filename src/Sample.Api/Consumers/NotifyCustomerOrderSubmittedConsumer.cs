using MassTransit;
using Sample.Api.Contracts;

namespace Sample.Api.Consumers;

public class NotifyCustomerOrderSubmittedConsumer :
    IConsumer<OrderSubmitted>
{
    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        return Task.CompletedTask;
    }
}