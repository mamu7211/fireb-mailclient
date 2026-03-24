namespace Feirb.Api.Services;

public class ImapSyncSettings
{
    public const string SectionName = "ImapSync";

    public int SaveBatchSize { get; set; } = 50;
}
