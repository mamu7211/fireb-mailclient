namespace Feirb.Shared.Admin.Jobs;

public record JobExecutionStatsResponse(
    List<JobExecutionDayStats> TimeSeries);

public record JobExecutionDayStats(
    string Label,
    int SuccessCount,
    int FailedCount,
    int CancelledCount);
