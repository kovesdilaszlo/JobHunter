
using JobHunter.Application.DTOs;
using JobHunter.Domain.Enums;

namespace JobHunter.Application.Interfaces;

public interface IApiQuotaService
{
    Task<bool> TryConsumeAsync(
        JobSourceType sourceType,
        CancellationToken cancellationToken = default);

    Task<ApiQuotaDto> GetQuotaAsync(
        JobSourceType sourceType,
        CancellationToken cancellationToken = default);
}
