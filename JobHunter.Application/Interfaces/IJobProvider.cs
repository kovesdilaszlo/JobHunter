
using JobHunter.Application.DTOs;
using JobHunter.Domain.Enums;

namespace JobHunter.Application.Interfaces;

public interface IJobProvider
{
    string Name { get; }

    JobSourceType SourceType { get; }

    Task<IReadOnlyList<ExternalJobDto>> SearchAsync(
        JobProviderSearchRequest request,
        CancellationToken cancellationToken = default);
}
