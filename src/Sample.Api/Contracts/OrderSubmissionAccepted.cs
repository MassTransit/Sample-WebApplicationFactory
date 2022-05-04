namespace Sample.Api.Contracts;

public record OrderSubmissionAccepted
{
    public Guid OrderId { get; init; }
}