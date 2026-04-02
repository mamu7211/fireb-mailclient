namespace Feirb.Api.Data.Entities;

public class ClassificationResult
{
    public Guid Id { get; set; }
    public Guid CachedMessageId { get; set; }
    public CachedMessage CachedMessage { get; set; } = null!;
    public required string Result { get; set; }
    public DateTimeOffset ClassifiedAt { get; set; }
}
