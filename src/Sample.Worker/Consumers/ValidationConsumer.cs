using MassTransit;
using Sample.Contracts;

namespace Sample.Worker.Consumers;

public class ValidationConsumer :
    IConsumer<ValidateOrder>
{
    readonly ILogger<ValidationConsumer> _logger;

    public ValidationConsumer(ILogger<ValidationConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ValidateOrder> context)
    {
        _logger.LogInformation("Order Validated: {OrderId}", context.Message.OrderId);

        await Task.Delay(1000);

        await context.RespondAsync(new OrderValidated(context.Message.OrderId));
    }
}