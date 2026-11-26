
using JobHunter.Domain.Enums;

namespace JobHunter.Application.DTOs;

public sealed record JobImportResult(
    JobSourceType SourceType,
    int RetrievedCount,
    int InsertedCount,
    int DuplicateCount,
    int AddedSourceCount,
    int PossibleDuplicateCount,
    int FailedCount,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    IReadOnlyList<JobImportError> Errors)
{
    public TimeSpan Duration =>
        CompletedAt - StartedAt;

    public bool IsSuccessful =>
        FailedCount == 0;

    public bool IsPartiallySuccessful =>
        FailedCount > 0 &&
        InsertedCount + DuplicateCount > 0;
}
