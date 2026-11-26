
using JobHunter.Application.DTOs;
using JobHunter.Domain.Enums;

namespace JobHunter.Application.Interfaces;

public interface IJobImportService
{
    Task<JobImportResult> ImportAsync(
        JobSourceType sourceType,
        JobProviderSearchRequest request,
        CancellationToken cancellationToken = default);
}
