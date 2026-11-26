
using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;
using Xunit;

namespace JobHunter.Test.Domain.Entities;

public class SearchExecutionFailureTests
{
    private static SearchExecution CreateExecution()
    {
        var execution = new SearchExecution(
            Guid.NewGuid(),
            "Jooble",
            SearchTriggerType.Manual);

        execution.Start();

        return execution;
    }

    [Fact]
    public void Fail_WithCounts_PreservesStatistics()
    {
        var execution = CreateExecution();

        execution.Fail(
            "All results were invalid.",
            10,
            0,
            0);

        Assert.Equal(
            SearchExecutionStatus.Failed,
            execution.Status);

        Assert.Equal(10, execution.RetrievedCount);
        Assert.Equal(0, execution.InsertedCount);
        Assert.Equal(0, execution.DuplicateCount);

        Assert.NotNull(execution.CompletedAt);
        Assert.NotNull(execution.ErrorMessage);
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(10, -1, 0)]
    [InlineData(10, 0, -1)]
    [InlineData(10, 8, 5)]
    public void Fail_InvalidCounts_ThrowsException(
        int retrieved,
        int inserted,
        int duplicates)
    {
        var execution = CreateExecution();

        Assert.ThrowsAny<ArgumentException>(
            () => execution.Fail(
                "Import failed.",
                retrieved,
                inserted,
                duplicates));

        Assert.Equal(
            SearchExecutionStatus.Running,
            execution.Status);

        Assert.Null(execution.CompletedAt);
    }

    [Fact]
    public void Fail_EmptyMessage_DoesNotModifyExecution()
    {
        var execution = CreateExecution();

        Assert.Throws<ArgumentException>(
            () => execution.Fail("", 10, 0, 0));

        Assert.Equal(
            SearchExecutionStatus.Running,
            execution.Status);

        Assert.Equal(0, execution.RetrievedCount);

        Assert.Null(execution.CompletedAt);
    }
}
