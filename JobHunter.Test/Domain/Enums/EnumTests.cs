
using JobHunter.Domain.Enums;

namespace JobHunter.Test.Domain.Enums;

public class EnumTests
{
    [Theory]
    [InlineData(ApplicationStatus.New)]
    [InlineData(ApplicationStatus.Saved)]
    [InlineData(ApplicationStatus.Applied)]
    [InlineData(ApplicationStatus.Interview)]
    [InlineData(ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.OfferReceived)]
    public void ApplicationStatus_AllValues_AreDefined(
        ApplicationStatus status)
    {
        Assert.True(Enum.IsDefined(status));
    }

    [Theory]
    [InlineData(JobSourceType.Manual)]
    [InlineData(JobSourceType.Jooble)]
    [InlineData(JobSourceType.LinkedIn)]
    [InlineData(JobSourceType.NoFluffJobs)]
    public void JobSourceType_AllValues_AreDefined(
        JobSourceType source)
    {
        Assert.True(Enum.IsDefined(source));
    }

    [Theory]
    [InlineData(WorkArrangement.Unknown)]
    [InlineData(WorkArrangement.OnSite)]
    [InlineData(WorkArrangement.Hybrid)]
    [InlineData(WorkArrangement.Remote)]
    public void WorkArrangement_AllValues_AreDefined(
        WorkArrangement arrangement)
    {
        Assert.True(Enum.IsDefined(arrangement));
    }

    [Theory]
    [InlineData(SearchExecutionStatus.Pending)]
    [InlineData(SearchExecutionStatus.Running)]
    [InlineData(SearchExecutionStatus.Completed)]
    [InlineData(SearchExecutionStatus.Failed)]
    [InlineData(SearchExecutionStatus.PartiallyCompleted)]
    [InlineData(SearchExecutionStatus.Cancelled)]
    public void SearchExecutionStatus_AllValues_AreDefined(
        SearchExecutionStatus status)
    {
        Assert.True(Enum.IsDefined(status));
    }


    [Theory]
    [InlineData(SearchTriggerType.Manual)]
    [InlineData(SearchTriggerType.Startup)]
    [InlineData(SearchTriggerType.Scheduled)]
    public void SearchTriggerType_AllValues_AreDefined(
        SearchTriggerType triggerType)
    {
        Assert.True(Enum.IsDefined(triggerType));
    }

}
