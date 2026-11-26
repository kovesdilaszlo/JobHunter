
using JobHunter.Application.Exceptions;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Enums;

namespace JobHunter.Infrastructure.Providers.Jooble;

public sealed class JoobleQuotaHandler
    : DelegatingHandler
{
    private readonly IApiQuotaService _quotaService;

    public JoobleQuotaHandler(
        IApiQuotaService quotaService)
    {
        _quotaService = quotaService
            ?? throw new ArgumentNullException(
                nameof(quotaService));
    }

    protected override async Task<HttpResponseMessage>
        SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var allowed =
            await _quotaService.TryConsumeAsync(
                JobSourceType.Jooble,
                cancellationToken);

        if (!allowed)
        {
            var quota =
                await _quotaService.GetQuotaAsync(
                    JobSourceType.Jooble,
                    cancellationToken);

            throw new ApiQuotaExceededException(quota);
        }

        return await base.SendAsync(
            request,
            cancellationToken);
    }
}
