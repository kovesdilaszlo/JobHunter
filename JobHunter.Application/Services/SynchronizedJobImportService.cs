
using JobHunter.Application.DTOs;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Enums;

namespace JobHunter.Application.Services;

public sealed class SynchronizedJobImportService
    : IJobImportService
{
    private readonly IJobImportService _innerService;

    private readonly IJobImportLock _importLock;

    public SynchronizedJobImportService(
        IJobImportService innerService,
        IJobImportLock importLock)
    {
        _innerService = innerService
            ?? throw new ArgumentNullException(
                nameof(innerService));

        _importLock = importLock
            ?? throw new ArgumentNullException(
                nameof(importLock));
    }

    public async Task<JobImportResult> ImportAsync(
        JobSourceType sourceType,
        JobProviderSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var lockHandle =
            await _importLock.AcquireAsync(
                cancellationToken);

        return await _innerService.ImportAsync(
            sourceType,
            request,
            cancellationToken);
    }
}
