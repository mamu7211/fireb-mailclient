namespace Feirb.Api.Data.Entities;

public class CachedMessageClassificationQueueItem
{
    public Guid Id { get; set; }
    public Guid CachedMessageId { get; set; }
    public CachedMessage CachedMessage { get; set; } = null!;
    public ClassificationQueueItemStatus Status { get; set; }
    public int Ordinal { get; set; }
    public string? Error { get; set; }
    public int AttemptNumber { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
