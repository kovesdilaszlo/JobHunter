
using JobHunter.Application.DTOs;

namespace JobHunter.Application.Interfaces;

public interface ISearchExecutionService
{
    Task<SearchExecutionDto> ExecuteAsync(
        StartSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<SearchExecutionDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchExecutionDto>> GetRecentAsync(
        int count = 50,
        CancellationToken cancellationToken = default);
}
