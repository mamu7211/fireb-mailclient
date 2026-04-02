using Feirb.Api.Data.Entities;

namespace Feirb.Api.Services;

public class NoopClassificationService : IClassificationService
{
    public Task<ClassificationServiceResult> ClassifyAsync(CachedMessage message, CancellationToken cancellationToken = default) =>
        Task.FromResult(new ClassificationServiceResult(Success: true, Result: "noop-classification", Error: null));
}
