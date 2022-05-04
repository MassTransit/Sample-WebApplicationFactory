namespace Sample.Api.Contracts;

public record OrderSubmitted
{
    public Guid OrderId { get; init; }
}