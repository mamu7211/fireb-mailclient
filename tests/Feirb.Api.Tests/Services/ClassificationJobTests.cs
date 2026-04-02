using Feirb.Api.Data;
using Feirb.Api.Data.Entities;
using Feirb.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Quartz;

namespace Feirb.Api.Tests.Services;

public class ClassificationJobTests : IDisposable
{
    private const string _testJobName = "TestClassification";
    private readonly ServiceProvider _serviceProvider;
    private readonly DbContextOptions<FeirbDbContext> _dbOptions;
    private readonly Guid _jobSettingsId = Guid.NewGuid();
    private readonly Guid _mailboxId = Guid.NewGuid();

    public ClassificationJobTests()
    {
        var dbName = $"ClassificationJobTests-{Guid.NewGuid()}";
        _dbOptions = new DbContextOptionsBuilder<FeirbDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var services = new ServiceCollection();
        services.AddSingleton(_dbOptions);
        services.AddScoped(sp => new FeirbDbContext(sp.GetRequiredService<DbContextOptions<FeirbDbContext>>()));
        services.AddSingleton<IJobSettingsScheduler, NoOpJobSettingsScheduler>();
        services.AddScoped<IClassificationService, NoopClassificationService>();
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();

        using var db = new FeirbDbContext(_dbOptions);
        db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Execute_ProcessesPendingItems_ClassifiesAndWritesResultAsync()
    {
        var messageId = SeedMessageWithQueueItem();
        SeedJobSettings();

        var job = CreateJob();
        await job.Execute(CreateJobContext());

        using var db = new FeirbDbContext(_dbOptions);
        var queueItem = await db.ClassificationQueueItems
            .AsNoTracking()
            .FirstAsync(q => q.CachedMessageId == messageId);
        queueItem.Status.Should().Be(ClassificationQueueItemStatus.Classified);

        var result = await db.ClassificationResults
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.CachedMessageId == messageId);
        result.Should().NotBeNull();
        result!.Result.Should().Be("noop-classification");
        result.ClassifiedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Execute_NoPendingItems_CompletesWithoutErrorAsync()
    {
        SeedJobSettings();

        var job = CreateJob();
        var act = () => job.Execute(CreateJobContext());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Execute_RespectsBatchSizeFromConfigurationAsync()
    {
        SeedJobSettings(configuration: """{"batchSize":1}""");
        SeedMessageWithQueueItem();
        SeedMessageWithQueueItem();

        var job = CreateJob();
        await job.Execute(CreateJobContext());

        using var db = new FeirbDbContext(_dbOptions);
        var classified = await db.ClassificationQueueItems
            .CountAsync(q => q.Status == ClassificationQueueItemStatus.Classified);
        var pending = await db.ClassificationQueueItems
            .CountAsync(q => q.Status == ClassificationQueueItemStatus.Pending);

        classified.Should().Be(1);
        pending.Should().Be(1);
    }

    [Fact]
    public async Task Execute_ClassificationFails_SetsFailedStatusWithErrorAsync()
    {
        var messageId = SeedMessageWithQueueItem();
        SeedJobSettings();

        // Replace the classification service with a failing one
        var services = new ServiceCollection();
        services.AddSingleton(_dbOptions);
        services.AddScoped(sp => new FeirbDbContext(sp.GetRequiredService<DbContextOptions<FeirbDbContext>>()));
        services.AddSingleton<IJobSettingsScheduler, NoOpJobSettingsScheduler>();
        services.AddScoped<IClassificationService>(_ => new FailingClassificationService());
        services.AddLogging();

        using var failingProvider = services.BuildServiceProvider();
        var scopeFactory = failingProvider.GetRequiredService<IServiceScopeFactory>();
        var logger = NullLogger<ClassificationJob>.Instance;
        var job = new ClassificationJob(scopeFactory, logger);

        await job.Execute(CreateJobContext());

        using var db = new FeirbDbContext(_dbOptions);
        var queueItem = await db.ClassificationQueueItems
            .AsNoTracking()
            .FirstAsync(q => q.CachedMessageId == messageId);
        queueItem.Status.Should().Be(ClassificationQueueItemStatus.Failed);
        queueItem.Error.Should().NotBeNullOrEmpty();
    }

    private Guid SeedMessageWithQueueItem()
    {
        using var db = new FeirbDbContext(_dbOptions);

        // Ensure mailbox exists
        if (!db.Mailboxes.Any(m => m.Id == _mailboxId))
        {
            db.Mailboxes.Add(new Mailbox
            {
                Id = _mailboxId,
                UserId = Guid.NewGuid(),
                Name = "Test Mailbox",
                EmailAddress = "test@example.com",
                ImapHost = "localhost",
                ImapPort = 993,
                ImapUsername = "test",
                SmtpHost = "localhost",
                SmtpPort = 587,
                SmtpUsername = "test",
            });
            db.SaveChanges();
        }

        var messageId = Guid.NewGuid();
        var message = new CachedMessage
        {
            Id = messageId,
            MailboxId = _mailboxId,
            MessageId = $"<{Guid.NewGuid()}@test>",
            Subject = "Test message",
            From = "sender@example.com",
            To = "recipient@example.com",
            Date = DateTimeOffset.UtcNow,
            SyncedAt = DateTimeOffset.UtcNow,
        };
        db.CachedMessages.Add(message);

        db.ClassificationQueueItems.Add(new CachedMessageClassificationQueueItem
        {
            Id = Guid.NewGuid(),
            CachedMessageId = messageId,
            Status = ClassificationQueueItemStatus.Pending,
            AttemptNumber = 1,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        db.SaveChanges();
        return messageId;
    }

    private void SeedJobSettings(string? configuration = null)
    {
        using var db = new FeirbDbContext(_dbOptions);
        if (db.JobSettings.Any(j => j.Id == _jobSettingsId))
            return;

        db.JobSettings.Add(new JobSettings
        {
            Id = _jobSettingsId,
            JobName = _testJobName,
            JobType = "classification",
            Description = "Test classification job",
            Cron = "0 * * * * ?",
            Enabled = true,
            Configuration = configuration ?? """{"batchSize":10}""",
        });
        db.SaveChanges();
    }

    private ClassificationJob CreateJob()
    {
        var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var logger = NullLogger<ClassificationJob>.Instance;
        return new ClassificationJob(scopeFactory, logger);
    }

    private static IJobExecutionContext CreateJobContext()
    {
        var dataMap = new JobDataMap { { ManagedJob.JobNameKey, _testJobName } };
        var context = Substitute.For<IJobExecutionContext>();
        context.MergedJobDataMap.Returns(dataMap);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }

    private sealed class FailingClassificationService : IClassificationService
    {
        public Task<ClassificationServiceResult> ClassifyAsync(
            CachedMessage message, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ClassificationServiceResult(Success: false, Result: null, Error: "Classification failed"));
    }
}
