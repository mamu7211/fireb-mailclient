using Feirb.Api.Data;
using Feirb.Api.Data.Entities;
using Feirb.Shared.Admin.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Feirb.Api.Endpoints;

public static class JobStatsEndpoints
{
    public static RouteGroupBuilder MapJobStatsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/stats", GetExecutionStatsAsync);
        group.MapGet("/unhealthy", GetUnhealthyJobsAsync);
        return group;
    }

    private static async Task<IResult> GetExecutionStatsAsync(
        int? days,
        FeirbDbContext db)
    {
        var range = days is > 0 and <= 365 ? days.Value : 7;
        var sinceDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-range);
        var since = new DateTimeOffset(sinceDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        // Fetch filtered rows and group in memory to avoid DateTimeOffset translation issues with PostgreSQL
        var executions = await db.JobExecutions
            .Where(e => e.StartedAt >= since)
            .Select(e => new { e.StartedAt, e.Status })
            .ToListAsync();

        var grouped = executions
            .GroupBy(e => DateOnly.FromDateTime(e.StartedAt.UtcDateTime))
            .ToDictionary(
                g => g.Key,
                g => (
                    Success: g.Count(e => e.Status == JobExecutionStatus.Success),
                    Failed: g.Count(e => e.Status == JobExecutionStatus.Failed),
                    Cancelled: g.Count(e => e.Status == JobExecutionStatus.Cancelled)));

        var timeSeries = new List<JobExecutionDayStats>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        for (var date = sinceDate; date <= today; date = date.AddDays(1))
        {
            var counts = grouped.GetValueOrDefault(date);
            timeSeries.Add(new JobExecutionDayStats(
                date.ToString("yyyy-MM-dd"),
                counts.Success,
                counts.Failed,
                counts.Cancelled));
        }

        return Results.Ok(new JobExecutionStatsResponse(timeSeries));
    }

    private static async Task<IResult> GetUnhealthyJobsAsync(
        int? page,
        int? pageSize,
        FeirbDbContext db)
    {
        var currentPage = page is > 0 ? page.Value : 1;
        var currentPageSize = pageSize is > 0 and <= 100 ? pageSize.Value : 5;

        var query = db.JobSettings
            .Where(j => j.LastStatus == JobExecutionStatus.Failed || !j.Enabled);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(j => j.LastRunAt)
            .Skip((currentPage - 1) * currentPageSize)
            .Take(currentPageSize)
            .Select(j => new UnhealthyJobEntry(
                j.Id,
                j.JobName,
                j.LastRunAt,
                !j.Enabled ? "Disabled" : "Failed",
                j.Executions
                    .Where(e => e.Status == JobExecutionStatus.Failed)
                    .OrderByDescending(e => e.StartedAt)
                    .Select(e => e.Error)
                    .FirstOrDefault()))
            .ToListAsync();

        return Results.Ok(new UnhealthyJobsResponse(items, totalCount));
    }
}
