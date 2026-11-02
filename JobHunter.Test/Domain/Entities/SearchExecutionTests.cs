
using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;

namespace JobHunter.Test.Domain.Entities;

public class SearchExecutionTests
{
    private static SearchExecution CreateExecution()
    {
        return new SearchExecution(
            Guid.NewGuid(),
            "Jooble",
            SearchTriggerType.Scheduled);
    }

    [Fact]
    public void Constructor_ValidData_CreatesPendingExecution()
    {
        var profileId = Guid.NewGuid();

        var execution = new SearchExecution(
            profileId,
            "  Jooble  ",
            SearchTriggerType.Manual);

        Assert.NotEqual(Guid.Empty, execution.Id);
        Assert.Equal(profileId, execution.JobSearchProfileId);
        Assert.Equal("Jooble", execution.ProviderName);
        Assert.Equal(SearchTriggerType.Manual, execution.TriggerType);
        Assert.Equal(SearchExecutionStatus.Pending, execution.Status);
        Assert.Null(execution.CompletedAt);
        Assert.Null(execution.ErrorMessage);
        Assert.Equal(0, execution.RetrievedCount);
        Assert.Equal(0, execution.InsertedCount);
        Assert.Equal(0, execution.DuplicateCount);
    }

    [Fact]
    public void Constructor_EmptyProfileId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new SearchExecution(
                Guid.Empty,
                "Jooble",
                SearchTriggerType.Manual));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_EmptyProviderName_ThrowsException(
        string provider)
    {
        Assert.Throws<ArgumentException>(() =>
            new SearchExecution(
                Guid.NewGuid(),
                provider,
                SearchTriggerType.Manual));
    }

    [Fact]
    public void Constructor_InvalidTriggerType_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SearchExecution(
                Guid.NewGuid(),
                "Jooble",
                (SearchTriggerType)999));
    }

    [Fact]
    public void Start_PendingExecution_SetsRunningStatus()
    {
        var execution = CreateExecution();

        execution.Start();

        Assert.Equal(
            SearchExecutionStatus.Running,
            execution.Status);
    }

    [Fact]
    public void Start_RunningExecution_ThrowsException()
    {
        var execution = CreateExecution();
        execution.Start();

        Assert.Throws<InvalidOperationException>(() =>
            execution.Start());
    }

    [Fact]
    public void Complete_RunningExecution_SetsResults()
    {
        var execution = CreateExecution();
        execution.Start();

        execution.Complete(20, 15, 5);

        Assert.Equal(
            SearchExecutionStatus.Completed,
            execution.Status);
        Assert.Equal(20, execution.RetrievedCount);
        Assert.Equal(15, execution.InsertedCount);
        Assert.Equal(5, execution.DuplicateCount);
        Assert.NotNull(execution.CompletedAt);
        Assert.Null(execution.ErrorMessage);
    }

    [Fact]
    public void Complete_PendingExecution_ThrowsException()
    {
        var execution = CreateExecution();

        Assert.Throws<InvalidOperationException>(() =>
            execution.Complete(10, 8, 2));
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(10, -1, 0)]
    [InlineData(10, 0, -1)]
    [InlineData(10, 8, 5)]
    public void Complete_InvalidCounts_ThrowsException(
        int retrieved,
        int inserted,
        int duplicates)
    {
        var execution = CreateExecution();
        execution.Start();

        Assert.ThrowsAny<ArgumentException>(() =>
            execution.Complete(
                retrieved,
                inserted,
                duplicates));

        Assert.Equal(
            SearchExecutionStatus.Running,
            execution.Status);
    }

    [Fact]
    public void Fail_RunningExecution_SetsFailedStatus()
    {
        var execution = CreateExecution();
        execution.Start();

        execution.Fail("  API request failed  ");

        Assert.Equal(
            SearchExecutionStatus.Failed,
            execution.Status);
        Assert.Equal(
            "API request failed",
            execution.ErrorMessage);
        Assert.NotNull(execution.CompletedAt);
    }

    [Fact]
    public void Fail_PendingExecution_ThrowsException()
    {
        var execution = CreateExecution();

        Assert.Throws<InvalidOperationException>(() =>
            execution.Fail("API error"));
    }

    [Fact]
    public void Fail_EmptyMessage_ThrowsException()
    {
        var execution = CreateExecution();
        execution.Start();

        Assert.Throws<ArgumentException>(() =>
            execution.Fail(" "));
    }

    [Fact]
    public void CompletePartially_ValidData_SetsPartialStatus()
    {
        var execution = CreateExecution();
        execution.Start();

        execution.CompletePartially(
            10, 6, 2, "Two results were invalid");

        Assert.Equal(
            SearchExecutionStatus.PartiallyCompleted,
            execution.Status);
        Assert.Equal(10, execution.RetrievedCount);
        Assert.Equal(6, execution.InsertedCount);
        Assert.Equal(2, execution.DuplicateCount);
        Assert.Equal(
            "Two results were invalid",
            execution.ErrorMessage);
        Assert.NotNull(execution.CompletedAt);
    }

    [Fact]
    public void CompletePartially_EmptyError_ThrowsException()
    {
        var execution = CreateExecution();
        execution.Start();

        Assert.Throws<ArgumentException>(() =>
            execution.CompletePartially(
                10, 6, 2, ""));
    }

    [Fact]
    public void Cancel_PendingExecution_SetsCancelledStatus()
    {
        var execution = CreateExecution();

        execution.Cancel();

        Assert.Equal(
            SearchExecutionStatus.Cancelled,
            execution.Status);
        Assert.NotNull(execution.CompletedAt);
    }

    [Fact]
    public void Cancel_RunningExecution_SetsCancelledStatus()
    {
        var execution = CreateExecution();
        execution.Start();

        execution.Cancel();

        Assert.Equal(
            SearchExecutionStatus.Cancelled,
            execution.Status);
    }

    [Fact]
    public void Cancel_CompletedExecution_ThrowsException()
    {
        var execution = CreateExecution();
        execution.Start();
        execution.Complete(10, 8, 2);

        Assert.Throws<InvalidOperationException>(() =>
            execution.Cancel());
    }

    [Fact]
    public void Complete_AlreadyCompleted_ThrowsException()
    {
        var execution = CreateExecution();
        execution.Start();
        execution.Complete(10, 8, 2);

        Assert.Throws<InvalidOperationException>(() =>
            execution.Complete(20, 15, 5));
    }

    [Fact]
    public void Fail_AlreadyFailed_ThrowsException()
    {
        var execution = CreateExecution();
        execution.Start();
        execution.Fail("First failure");

        Assert.Throws<InvalidOperationException>(() =>
            execution.Fail("Second failure"));
    }

    [Fact]
    public void Start_CancelledExecution_ThrowsException()
    {
        var execution = CreateExecution();
        execution.Cancel();

        Assert.Throws<InvalidOperationException>(() =>
            execution.Start());
    }
}
