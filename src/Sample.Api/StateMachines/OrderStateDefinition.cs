using MassTransit;

namespace Sample.Api.StateMachines;

#pragma warning disable CS8618
public class OrderStateDefinition :
    SagaDefinition<OrderState>
{
    protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<OrderState> sagaConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 200, 500, 1000));
        endpointConfigurator.UseInMemoryOutbox(context);
    }
}