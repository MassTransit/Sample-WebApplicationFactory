using MassTransit;
using Sample.Contracts;

namespace Sample.Api.StateMachines;

public class OrderStateMachine :
    MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => SubmitOrder, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => OrderStatusRequested, x =>
        {
            x.CorrelateById(context => context.Message.OrderId);
            x.OnMissingInstance(m => m.ExecuteAsync(async context =>
            {
                if (context.IsResponseAccepted<OrderNotFound>())
                    await context.RespondAsync(new OrderNotFound(context.Message.OrderId));
            }));
        });

        Request(() => ValidateRequest, x => x.Timeout = TimeSpan.Zero);

        Initially(
            When(SubmitOrder)
                .TransitionTo(Submitted)
                .Publish(x => new OrderSubmitted(x.Saga.CorrelationId))
                .Respond(x => new OrderSubmissionAccepted(x.Saga.CorrelationId))
                .Request(ValidateRequest, x => new ValidateOrder(x.Saga.CorrelationId))
        );

        During(Submitted,
            When(ValidateRequest!.Completed)
                .Publish(x => new OrderAccepted(x.Saga.CorrelationId))
                .TransitionTo(Accepted),
            When(ValidateRequest.Faulted)
                .Publish(x => new OrderRejected(x.Saga.CorrelationId))
                .TransitionTo(Rejected));

        DuringAny(
            When(OrderStatusRequested)
                .RespondAsync(async x => new OrderStatus(x.Saga.CorrelationId, (await x.StateMachine.GetState(x)).Name))
        );
    }

    public State Submitted { get; } = null!;
    public State Accepted { get; } = null!;
    public State Rejected { get; } = null!;
    public Event<SubmitOrder> SubmitOrder { get; } = null!;
    public Event<GetOrderStatus> OrderStatusRequested { get; } = null!;
    public Request<OrderState, ValidateOrder, OrderValidated> ValidateRequest { get; } = null!;
}