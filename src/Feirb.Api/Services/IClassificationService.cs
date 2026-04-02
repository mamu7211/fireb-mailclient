using Feirb.Api.Data.Entities;

namespace Feirb.Api.Services;

public interface IClassificationService
{
    Task<ClassificationServiceResult> ClassifyAsync(CachedMessage message, CancellationToken cancellationToken = default);
}

public record ClassificationServiceResult(bool Success, string? Result, string? Error);
