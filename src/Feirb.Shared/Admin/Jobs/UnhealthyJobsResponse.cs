namespace Feirb.Shared.Admin.Jobs;

public record UnhealthyJobsResponse(
    List<UnhealthyJobEntry> Items,
    int TotalCount);

public record UnhealthyJobEntry(
    Guid Id,
    string JobName,
    DateTimeOffset? LastRunAt,
    string Status,
    string? LastError);
