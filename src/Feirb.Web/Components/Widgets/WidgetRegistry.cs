namespace Feirb.Web.Components.Widgets;

public static class WidgetRegistry
{
    public static IReadOnlyList<WidgetDefinition> All { get; } =
    [
        new WidgetDefinition("mail-count", "WidgetMailCountName", "WidgetMailCountDescription", typeof(TotalMailCountWidget), Icon: "bi-envelope", DefaultWidth: 3, DefaultHeight: 2),
        new WidgetDefinition("mails-per-day", "WidgetMailsPerDayName", "WidgetMailsPerDayDescription", typeof(MailsPerDayWidget), Icon: "bi-bar-chart", DefaultWidth: 6, DefaultHeight: 3, DefaultConfig: "7"),
        new WidgetDefinition("mailbox-doughnut", "WidgetMailboxDoughnutName", "WidgetMailboxDoughnutDescription", typeof(MailboxDoughnutWidget), Icon: "bi-pie-chart", DefaultWidth: 4, DefaultHeight: 3),
        new WidgetDefinition("job-executions", "WidgetJobExecutionsName", "WidgetJobExecutionsDescription", typeof(JobExecutionsPerDayWidget), Icon: "bi-bar-chart-steps", DefaultWidth: 6, DefaultHeight: 3, DefaultConfig: "7", RequiresAdmin: true),
        new WidgetDefinition("job-health", "WidgetJobHealthName", "WidgetJobHealthDescription", typeof(JobHealthWidget), Icon: "bi-heart-pulse", DefaultWidth: 12, DefaultHeight: 4, RequiresAdmin: true),
    ];

    public static WidgetDefinition? GetById(string id) =>
        All.FirstOrDefault(w => w.Id == id);
}
