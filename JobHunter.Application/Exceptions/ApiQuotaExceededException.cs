
using JobHunter.Application.DTOs;

namespace JobHunter.Application.Exceptions;

public sealed class ApiQuotaExceededException : Exception
{
    public ApiQuotaExceededException(ApiQuotaDto quota)
        : base(
            $"The daily API quota for {quota.SourceType} " +
            "has been exhausted.")
    {
        Quota = quota;
    }

    public ApiQuotaDto Quota { get; }
}
