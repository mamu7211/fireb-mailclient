using System.Text.Json;
using Feirb.Api.Data;
using Feirb.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Feirb.Api.Services;

public class ClassificationJob(IServiceScopeFactory scopeFactory, ILogger<ClassificationJob> logger)
    : ManagedJob(scopeFactory, logger)
{
    private const int _defaultBatchSize = 10;

    protected override async Task RunAsync(
        IServiceProvider serviceProvider, JobSettings jobSettings, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(jobSettings);

        var batchSize = GetBatchSize(jobSettings);
        var db = serviceProvider.GetRequiredService<FeirbDbContext>();
        var classificationService = serviceProvider.GetRequiredService<IClassificationService>();

        var pendingItems = await db.ClassificationQueueItems
            .Include(q => q.CachedMessage)
            .Where(q => q.Status == ClassificationQueueItemStatus.Pending)
            .OrderBy(q => q.Ordinal).ThenBy(q => q.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (pendingItems.Count == 0)
        {
            logger.LogDebug("No pending classification queue items found");
            return;
        }

        logger.LogInformation("Processing {Count} classification queue items", pendingItems.Count);

        foreach (var item in pendingItems)
        {
            item.Status = ClassificationQueueItemStatus.Processing;
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var item in pendingItems)
        {
            try
            {
                var result = await classificationService.ClassifyAsync(item.CachedMessage, cancellationToken);

                if (result.Success && result.Result is not null)
                {
                    item.Status = ClassificationQueueItemStatus.Classified;

                    db.ClassificationResults.Add(new ClassificationResult
                    {
                        Id = Guid.NewGuid(),
                        CachedMessageId = item.CachedMessageId,
                        Result = result.Result,
                        ClassifiedAt = DateTimeOffset.UtcNow,
                    });
                }
                else if (result.Success)
                {
                    item.Status = ClassificationQueueItemStatus.Failed;
                    item.Error = "Classification service returned success but no result";
                }
                else
                {
                    item.Status = ClassificationQueueItemStatus.Failed;
                    item.Error = result.Error;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Classification failed for queue item {QueueItemId}", item.Id);
                item.Status = ClassificationQueueItemStatus.Failed;
                item.Error = ex.Message.Length > 4096 ? ex.Message[..4096] : ex.Message;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var classified = pendingItems.Count(i => i.Status == ClassificationQueueItemStatus.Classified);
        var failed = pendingItems.Count(i => i.Status == ClassificationQueueItemStatus.Failed);
        logger.LogInformation("Classification complete: {Classified} classified, {Failed} failed", classified, failed);
    }

    private int GetBatchSize(JobSettings jobSettings)
    {
        if (string.IsNullOrEmpty(jobSettings.Configuration))
            return _defaultBatchSize;

        try
        {
            using var doc = JsonDocument.Parse(jobSettings.Configuration);
            if (doc.RootElement.TryGetProperty("batchSize", out var batchSizeElement)
                && batchSizeElement.TryGetInt32(out var batchSize)
                && batchSize > 0)
            {
                return batchSize;
            }
        }
        catch (JsonException)
        {
            logger.LogWarning(
                "Job '{JobName}' has invalid JSON in Configuration, using default batch size {BatchSize}",
                jobSettings.JobName, _defaultBatchSize);
        }

        return _defaultBatchSize;
    }
}
