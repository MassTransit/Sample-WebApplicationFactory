namespace Sample.Api.Contracts;

public record SubmitOrder
{
    public Guid OrderId { get; init; }
}