
using JobHunter.Domain.Enums;

namespace JobHunter.Domain.Entities;

public class SearchExecution
{
    private SearchExecution()
    {
        // Required by EF Core.
    }

    public SearchExecution(
        Guid jobSearchProfileId,
        string providerName,
        SearchTriggerType triggerType)
    {
        if (jobSearchProfileId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid search profile ID is required.",
                nameof(jobSearchProfileId));
        }

        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new ArgumentException(
                "Provider name cannot be empty.",
                nameof(providerName));
        }

        if (!Enum.IsDefined(triggerType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(triggerType));
        }

        Id = Guid.NewGuid();
        JobSearchProfileId = jobSearchProfileId;
        ProviderName = providerName.Trim();
        TriggerType = triggerType;

        Status = SearchExecutionStatus.Pending;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid JobSearchProfileId { get; private set; }

    public string ProviderName { get; private set; } = string.Empty;

    public SearchTriggerType TriggerType { get; private set; }

    public SearchExecutionStatus Status { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public int RetrievedCount { get; private set; }

    public int InsertedCount { get; private set; }

    public int DuplicateCount { get; private set; }

    public string? ErrorMessage { get; private set; }

    public void Start()
    {
        if (Status != SearchExecutionStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending executions can be started.");
        }

        Status = SearchExecutionStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void Complete(
        int retrievedCount,
        int insertedCount,
        int duplicateCount)
    {
        EnsureRunning();
        ValidateCounts(retrievedCount, insertedCount, duplicateCount);

        RetrievedCount = retrievedCount;
        InsertedCount = insertedCount;
        DuplicateCount = duplicateCount;

        Status = SearchExecutionStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        ErrorMessage = null;
    }

    public void Fail(string errorMessage)
    {
        EnsureRunning();

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException(
                "An error message is required.",
                nameof(errorMessage));
        }

        ErrorMessage = errorMessage.Trim();
        Status = SearchExecutionStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void CompletePartially(
        int retrievedCount,
        int insertedCount,
        int duplicateCount,
        string errorMessage)
    {
        EnsureRunning();
        ValidateCounts(retrievedCount, insertedCount, duplicateCount);

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException(
                "An error message is required.",
                nameof(errorMessage));
        }

        RetrievedCount = retrievedCount;
        InsertedCount = insertedCount;
        DuplicateCount = duplicateCount;
        ErrorMessage = errorMessage.Trim();

        Status = SearchExecutionStatus.PartiallyCompleted;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status != SearchExecutionStatus.Pending &&
            Status != SearchExecutionStatus.Running)
        {
            throw new InvalidOperationException(
                "Only pending or running executions can be cancelled.");
        }

        Status = SearchExecutionStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    private void EnsureRunning()
    {
        if (Status != SearchExecutionStatus.Running)
        {
            throw new InvalidOperationException(
                "Execution must be running.");
        }
    }

    private static void ValidateCounts(
        int retrievedCount,
        int insertedCount,
        int duplicateCount)
    {
        if (retrievedCount < 0 ||
            insertedCount < 0 ||
            duplicateCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(retrievedCount),
                "Counts cannot be negative.");
        }

        if ((long)insertedCount + duplicateCount > retrievedCount)
        {
            throw new ArgumentException(
                "Inserted and duplicate counts cannot exceed retrieved count.");
        }
    }
}
