namespace Feirb.Shared.Admin.Jobs;

public record JobExecutionResponse(
    Guid Id,
    DateTime StartedAt,
    DateTime? FinishedAt,
    string Status,
    string? Error);
