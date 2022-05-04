using MassTransit;

namespace Sample.Api.StateMachines;

public class OrderState :
    SagaStateMachineInstance,
    ISagaVersion
{
    public string? CurrentState { get; set; }

    public int Version { get; set; }
    public Guid CorrelationId { get; set; }
}