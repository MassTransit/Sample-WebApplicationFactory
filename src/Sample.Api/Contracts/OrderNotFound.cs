namespace Sample.Api.Contracts;

public record OrderNotFound
{
    public Guid OrderId { get; init; }
}