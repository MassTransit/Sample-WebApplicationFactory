using MassTransit;
using Sample.Api.Contracts;

namespace Sample.Api.StateMachines;

#pragma warning disable CS8618
public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Event(() => SubmitOrder, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => OrderStatusRequested, x =>
        {
            x.CorrelateById(context => context.Message.OrderId);
            x.OnMissingInstance(x => x.ExecuteAsync(context => context.RespondAsync<OrderNotFound>(new { context.Message.OrderId })));
        });

        InstanceState(x => x.CurrentState);

        Initially(
            When(SubmitOrder)
                .TransitionTo(Submitted)
                .RespondAsync(x => x.Init<OrderSubmissionAccepted>(new
                {
                    OrderId = x.Saga.CorrelationId
                }))
        );

        DuringAny(
            When(OrderStatusRequested)
                .RespondAsync(x => x.Init<OrderStatus>(new
                {
                    OrderId = x.Saga.CorrelationId,
                    Status = x.StateMachine.GetState(x)
                }))
        );
    }

    //
    // ReSharper disable UnassignedGetOnlyAutoProperty
    public State Submitted { get; }
    public Event<SubmitOrder> SubmitOrder { get; }
    public Event<GetOrderStatus> OrderStatusRequested { get; }
}