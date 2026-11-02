
using JobHunter.Application.DTOs;
using JobHunter.Domain.Enums;

namespace JobHunter.Application.Interfaces;

public interface IJobService
{
    Task<CreateJobResult> CreateAsync(
        CreateJobRequest request,
        CancellationToken cancellationToken = default);

    Task<JobDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ChangeStatusAsync(
        Guid id,
        ApplicationStatus status,
        CancellationToken cancellationToken = default);

    Task<bool> ArchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> RestoreAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
