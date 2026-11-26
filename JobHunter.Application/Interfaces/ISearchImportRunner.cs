
using JobHunter.Application.DTOs;
using JobHunter.Domain.Enums;

namespace JobHunter.Application.Interfaces;

public interface ISearchImportRunner
{
    Task<JobImportResult> RunAsync(
        JobSourceType sourceType,
        JobProviderSearchRequest request,
        CancellationToken cancellationToken = default);
}
