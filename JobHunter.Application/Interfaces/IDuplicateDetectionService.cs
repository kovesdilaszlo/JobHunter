
using JobHunter.Application.DTOs;

namespace JobHunter.Application.Interfaces;

public interface IDuplicateDetectionService
{
    Task<DuplicateCheckResult> CheckAsync(
        DuplicateCheckRequest request,
        CancellationToken cancellationToken = default);
}
