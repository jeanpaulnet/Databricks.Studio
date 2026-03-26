using Databricks.Studio.Entity.Entities;
using Databricks.Studio.Entity.Enumerations;
using Databricks.Studio.Managers;
using Databricks.Studio.Shared.DTOs.Analytics;
using Databricks.Studio.Shared.DTOs.AnalyticsRun;
using Databricks.Studio.UnitTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Databricks.Studio.UnitTests;

public class StudioManagerTests : IDisposable
{
    private readonly DbContextFixture _fixture = new();
    private readonly StudioManager _sut;

    public StudioManagerTests()
    {
        _sut = new StudioManager(_fixture.Context, NullLogger<StudioManager>.Instance);
    }

    // ════════════════════════════════════════════════════════════════════════
    // Analytics
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateAnalyticsAsync_ValidDto_ReturnsSuccess()
    {
        var result = await _sut.CreateAnalyticsAsync(new CreateAnalyticsDto("Test", "Desc"), "user");

        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Test");
        result.Data.Status.Should().Be((int)AnalyticsStatus.Draft);
    }

    [Fact]
    public async Task CreateAnalyticsAsync_RecordsHistory()
    {
        await _sut.CreateAnalyticsAsync(new CreateAnalyticsDto("Hist", ""), "user");

        _fixture.Context.History.Should().ContainSingle();
    }

    [Fact]
    public async Task UpdateAnalyticsAsync_ExistingId_ReturnsUpdated()
    {
        var created = (await _sut.CreateAnalyticsAsync(new CreateAnalyticsDto("Old", ""), "user")).Data!;

        var result = await _sut.UpdateAnalyticsAsync(created.Id, new UpdateAnalyticsDto("New", "New desc"), "user");

        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("New");
    }

    [Fact]
    public async Task UpdateAnalyticsAsync_NonExistentId_ReturnsFailure()
    {
        var result = await _sut.UpdateAnalyticsAsync(Guid.NewGuid(), new UpdateAnalyticsDto("X", "Y"), "user");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task DeleteAnalyticsAsync_ExistingId_ReturnsTrue()
    {
        var created = (await _sut.CreateAnalyticsAsync(new CreateAnalyticsDto("Delete Me", ""), "user")).Data!;

        var result = await _sut.DeleteAnalyticsAsync(created.Id, "user");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAnalyticsAsync_NonExistentId_ReturnsFailure()
    {
        var result = await _sut.DeleteAnalyticsAsync(Guid.NewGuid(), "user");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetAnalyticsByIdAsync_ExistingId_ReturnsDto()
    {
        var created = (await _sut.CreateAnalyticsAsync(new CreateAnalyticsDto("Find Me", ""), "user")).Data!;

        var result = await _sut.GetAnalyticsByIdAsync(created.Id);

        result.Success.Should().BeTrue();
        result.Data!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task ListAnalyticsAsync_ReturnsPagedResult()
    {
        await _sut.CreateAnalyticsAsync(new CreateAnalyticsDto("A", ""), "user");
        await _sut.CreateAnalyticsAsync(new CreateAnalyticsDto("B", ""), "user");
        await _sut.CreateAnalyticsAsync(new CreateAnalyticsDto("C", ""), "user");

        var result = await _sut.ListAnalyticsAsync(1, 2);

        result.Success.Should().BeTrue();
        result.Data!.TotalCount.Should().Be(3);
        result.Data.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task ApproveAnalyticsAsync_SubmittedAnalytics_ReturnsApproved()
    {
        var created = (await _sut.CreateAnalyticsAsync(new CreateAnalyticsDto("To Approve", ""), "user")).Data!;
        var entity = await _fixture.Context.Analytics.FindAsync(created.Id);
        entity!.Status = AnalyticsStatus.Submitted;
        await _fixture.Context.SaveChangesAsync();

        var result = await _sut.ApproveAnalyticsAsync(created.Id, new ReviewAnalyticsDto("reviewer", null));

        result.Success.Should().BeTrue();
        result.Data!.Status.Should().Be((int)AnalyticsStatus.Approved);
    }

    [Fact]
    public async Task ApproveAnalyticsAsync_DraftAnalytics_ReturnsFailure()
    {
        var created = (await _sut.CreateAnalyticsAsync(new CreateAnalyticsDto("Draft", ""), "user")).Data!;

        var result = await _sut.ApproveAnalyticsAsync(created.Id, new ReviewAnalyticsDto("reviewer", null));

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RejectAnalyticsAsync_SubmittedAnalytics_ReturnsRejected()
    {
        var created = (await _sut.CreateAnalyticsAsync(new CreateAnalyticsDto("To Reject", ""), "user")).Data!;
        var entity = await _fixture.Context.Analytics.FindAsync(created.Id);
        entity!.Status = AnalyticsStatus.Submitted;
        await _fixture.Context.SaveChangesAsync();

        var result = await _sut.RejectAnalyticsAsync(created.Id, new ReviewAnalyticsDto("reviewer", "Not good enough"));

        result.Success.Should().BeTrue();
        result.Data!.Status.Should().Be((int)AnalyticsStatus.Rejected);
    }

    // ════════════════════════════════════════════════════════════════════════
    // Analytics Runs
    // ════════════════════════════════════════════════════════════════════════

    private async Task<Guid> SeedPublishedAnalyticsAsync()
    {
        var entity = new AnalyticsEntity
        {
            Id = Guid.NewGuid(),
            Name = "Published",
            Description = "",
            Status = AnalyticsStatus.Published
        };
        _fixture.Context.Analytics.Add(entity);
        await _fixture.Context.SaveChangesAsync();
        return entity.Id;
    }

    [Fact]
    public async Task StartRunAsync_PublishedAnalytics_CreatesRun()
    {
        var analyticsId = await SeedPublishedAnalyticsAsync();

        var result = await _sut.StartRunAsync(analyticsId, new StartAnalyticsRunDto("job-001", "user"));

        result.Success.Should().BeTrue();
        result.Data!.AnalyticsId.Should().Be(analyticsId);
        result.Data.Status.Should().Be((int)AnalyticsRunStatus.Queued);
    }

    [Fact]
    public async Task StartRunAsync_NonPublishedAnalytics_ReturnsFailure()
    {
        var entity = new AnalyticsEntity { Id = Guid.NewGuid(), Name = "Draft", Status = AnalyticsStatus.Draft };
        _fixture.Context.Analytics.Add(entity);
        await _fixture.Context.SaveChangesAsync();

        var result = await _sut.StartRunAsync(entity.Id, new StartAnalyticsRunDto("job-X", "user"));

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Published");
    }

    [Fact]
    public async Task StartRunAsync_NonExistentAnalytics_ReturnsFailure()
    {
        var result = await _sut.StartRunAsync(Guid.NewGuid(), new StartAnalyticsRunDto("job-X", "user"));

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task StopRunAsync_ActiveRun_TerminatesRun()
    {
        var analyticsId = await SeedPublishedAnalyticsAsync();
        var started = (await _sut.StartRunAsync(analyticsId, new StartAnalyticsRunDto("job-stop", "user"))).Data!;

        var result = await _sut.StopRunAsync(started.Id, new StopAnalyticsRunDto("user"));

        result.Success.Should().BeTrue();
        result.Data!.Status.Should().Be((int)AnalyticsRunStatus.Terminated);
        result.Data.TerminatedOn.Should().NotBeNull();
    }

    [Fact]
    public async Task StopRunAsync_AlreadyTerminated_ReturnsFailure()
    {
        var analyticsId = await SeedPublishedAnalyticsAsync();
        var started = (await _sut.StartRunAsync(analyticsId, new StartAnalyticsRunDto("job-term", "user"))).Data!;
        await _sut.StopRunAsync(started.Id, new StopAnalyticsRunDto("user"));

        var result = await _sut.StopRunAsync(started.Id, new StopAnalyticsRunDto("user"));

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already finished");
    }

    [Fact]
    public async Task GetRunByIdAsync_ExistingRun_ReturnsDto()
    {
        var analyticsId = await SeedPublishedAnalyticsAsync();
        var started = (await _sut.StartRunAsync(analyticsId, new StartAnalyticsRunDto("job-get", "user"))).Data!;

        var result = await _sut.GetRunByIdAsync(started.Id);

        result.Success.Should().BeTrue();
        result.Data!.Id.Should().Be(started.Id);
    }

    [Fact]
    public async Task GetRunHistoryAsync_AfterStartAndStop_ReturnsTwoEntries()
    {
        var analyticsId = await SeedPublishedAnalyticsAsync();
        var started = (await _sut.StartRunAsync(analyticsId, new StartAnalyticsRunDto("job-hist", "user"))).Data!;
        await _sut.StopRunAsync(started.Id, new StopAnalyticsRunDto("user"));

        var result = await _sut.GetRunHistoryAsync(analyticsId);

        result.Success.Should().BeTrue();
        result.Data!.Should().HaveCountGreaterOrEqualTo(2);
    }

    public void Dispose() => _fixture.Dispose();
}
