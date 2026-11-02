
namespace JobHunter.Domain.Enums;

public enum SearchExecutionStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    PartiallyCompleted = 4,
    Cancelled = 5
}
