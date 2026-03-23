namespace Feirb.Api.Data.Entities;

public class CachedAttachment
{
    public Guid Id { get; set; }
    public Guid CachedMessageId { get; set; }
    public CachedMessage CachedMessage { get; set; } = null!;
    public required string Filename { get; set; }
    public long Size { get; set; }
    public required string MimeType { get; set; }
}
