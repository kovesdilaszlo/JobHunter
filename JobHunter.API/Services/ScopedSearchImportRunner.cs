
using JobHunter.Application.DTOs;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace JobHunter.API.Services;

public sealed class ScopedSearchImportRunner
    : ISearchImportRunner
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ScopedSearchImportRunner(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory
            ?? throw new ArgumentNullException(
                nameof(scopeFactory));
    }

    public async Task<JobImportResult> RunAsync(
        JobSourceType sourceType,
        JobProviderSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var importService = scope.ServiceProvider
            .GetRequiredService<IJobImportService>();

        return await importService.ImportAsync(
            sourceType,
            request,
            cancellationToken);
    }
}
