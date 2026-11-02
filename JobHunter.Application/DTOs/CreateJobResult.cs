
using JobHunter.Application.Enums;

namespace JobHunter.Application.DTOs;

public sealed record CreateJobResult(
    JobDto Job,
    bool WasCreated,
    bool SourceWasAdded,
    DuplicateMatchType MatchType,
    Guid? PossibleDuplicateJobId);
