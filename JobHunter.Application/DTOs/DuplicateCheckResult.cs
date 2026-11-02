
using JobHunter.Application.Enums;

namespace JobHunter.Application.DTOs;

public sealed record DuplicateCheckResult(
    DuplicateMatchType MatchType,
    Guid? ExistingJobId,
    string Reason)
{
    public bool IsConfirmedDuplicate =>
        MatchType is DuplicateMatchType.ExternalId
            or DuplicateMatchType.NormalizedUrl;

    public bool IsPossibleDuplicate =>
        MatchType == DuplicateMatchType.PossibleMatch;
}
