namespace WebApi_SSE.Models
{
    public record Order(
        Guid Id,
        string ExternalId,
        string UserEmail,
        string CustomerName,
        decimal Amount,
        DateTime Timestamp
    );
}
