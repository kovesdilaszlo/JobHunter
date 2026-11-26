
using System.Net;

namespace JobHunter.Infrastructure.Providers.Jooble;

public sealed class JoobleApiException : Exception
{
    public JoobleApiException(
        string message,
        HttpStatusCode? statusCode = null)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode? StatusCode { get; }
}
