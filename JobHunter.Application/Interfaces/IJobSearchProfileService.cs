
using JobHunter.Application.DTOs;

namespace JobHunter.Application.Interfaces;

public interface IJobSearchProfileService
{
    Task<SearchProfileDto> CreateAsync(
        CreateSearchProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<SearchProfileDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchProfileDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchProfileDto>> GetEnabledAsync(
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        Guid id,
        UpdateSearchProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> EnableAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> DisableAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
