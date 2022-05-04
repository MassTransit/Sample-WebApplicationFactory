namespace Sample.Api.Contracts;

public record GetOrderStatus
{
    public Guid OrderId { get; init; }
}