
using System.Net;
using JobHunter.Application.DTOs;
using JobHunter.Application.Exceptions;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Enums;
using JobHunter.Infrastructure.Providers.Jooble;
using Xunit;

namespace JobHunter.Test.Infrastructure.Providers;

public class JoobleQuotaHandlerTests
{
    private static HttpClient CreateClient(
        FakeQuotaService quotaService,
        FakeHttpHandler httpHandler)
    {
        var handler = new JoobleQuotaHandler(
            quotaService)
        {
            InnerHandler = httpHandler
        };

        return new HttpClient(handler)
        {
            BaseAddress =
                new Uri("https://example.com/")
        };
    }

    [Fact]
    public async Task SendAsync_AvailableQuota_SendsRequest()
    {
        var quotaService = new FakeQuotaService();

        var httpHandler = new FakeHttpHandler();

        using var client = CreateClient(
            quotaService,
            httpHandler);

        using var response =
            await client.GetAsync("jobs");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.Equal(1, quotaService.ConsumeCalls);

        Assert.Equal(1, httpHandler.RequestCount);
    }

    [Fact]
    public async Task SendAsync_ExhaustedQuota_ThrowsException()
    {
        var quotaService = new FakeQuotaService
        {
            AllowRequest = false
        };

        var httpHandler = new FakeHttpHandler();

        using var client = CreateClient(
            quotaService,
            httpHandler);

        var exception =
            await Assert.ThrowsAsync<
                ApiQuotaExceededException>(
                () => client.GetAsync("jobs"));

        Assert.True(exception.Quota.IsExhausted);

        Assert.Equal(1, quotaService.ConsumeCalls);

        Assert.Equal(0, httpHandler.RequestCount);
    }

    [Fact]
    public async Task SendAsync_MultipleRequests_ConsumesEachRequest()
    {
        var quotaService = new FakeQuotaService();

        var httpHandler = new FakeHttpHandler();

        using var client = CreateClient(
            quotaService,
            httpHandler);

        for (var i = 0; i < 5; i++)
        {
            using var response =
                await client.GetAsync("jobs");
        }

        Assert.Equal(5, quotaService.ConsumeCalls);

        Assert.Equal(5, httpHandler.RequestCount);
    }

    [Fact]
    public async Task SendAsync_HttpError_StillConsumesQuota()
    {
        var quotaService = new FakeQuotaService();

        var httpHandler = new FakeHttpHandler
        {
            StatusCode =
                HttpStatusCode.InternalServerError
        };

        using var client = CreateClient(
            quotaService,
            httpHandler);

        using var response =
            await client.GetAsync("jobs");

        Assert.Equal(
            HttpStatusCode.InternalServerError,
            response.StatusCode);

        Assert.Equal(1, quotaService.ConsumeCalls);

        Assert.Equal(1, httpHandler.RequestCount);
    }

    [Fact]
    public async Task SendAsync_QuotaStorageFailure_DoesNotSendRequest()
    {
        var quotaService = new FakeQuotaService
        {
            ThrowException = true
        };

        var httpHandler = new FakeHttpHandler();

        using var client = CreateClient(
            quotaService,
            httpHandler);

        await Assert.ThrowsAsync<
            InvalidOperationException>(
            () => client.GetAsync("jobs"));

        Assert.Equal(0, httpHandler.RequestCount);
    }

    [Fact]
    public async Task SendAsync_CancelledToken_DoesNotSendRequest()
    {
        var quotaService = new FakeQuotaService();

        var httpHandler = new FakeHttpHandler();

        using var client = CreateClient(
            quotaService,
            httpHandler);

        using var cancellation =
            new CancellationTokenSource();

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<
            OperationCanceledException>(
            () => client.GetAsync(
                "jobs",
                cancellation.Token));

        Assert.Equal(0, quotaService.ConsumeCalls);

        Assert.Equal(0, httpHandler.RequestCount);
    }

    [Fact]
    public async Task SendAsync_UsesJoobleSourceType()
    {
        var quotaService = new FakeQuotaService();

        using var client = CreateClient(
            quotaService,
            new FakeHttpHandler());

        using var response =
            await client.GetAsync("jobs");

        Assert.Equal(
            JobSourceType.Jooble,
            quotaService.LastSourceType);
    }

    private sealed class FakeQuotaService
        : IApiQuotaService
    {
        public bool AllowRequest { get; set; }
            = true;

        public bool ThrowException { get; set; }

        public int ConsumeCalls { get; private set; }

        public JobSourceType LastSourceType
        {
            get;
            private set;
        }

        public Task<bool> TryConsumeAsync(
            JobSourceType sourceType,
            CancellationToken cancellationToken = default)
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            ConsumeCalls++;

            LastSourceType = sourceType;

            if (ThrowException)
            {
                throw new InvalidOperationException(
                    "Quota storage unavailable.");
            }

            return Task.FromResult(AllowRequest);
        }

        public Task<ApiQuotaDto> GetQuotaAsync(
            JobSourceType sourceType,
            CancellationToken cancellationToken = default)
        {
            var result = new ApiQuotaDto(
                sourceType,
                new DateOnly(2026, 11, 26),
                100,
                AllowRequest ? 0 : 100,
                null);

            return Task.FromResult(result);
        }
    }

    private sealed class FakeHttpHandler
        : HttpMessageHandler
    {
        public int RequestCount
        {
            get;
            private set;
        }

        public HttpStatusCode StatusCode
        {
            get;
            set;
        } = HttpStatusCode.OK;

        protected override Task<HttpResponseMessage>
            SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            RequestCount++;

            return Task.FromResult(
                new HttpResponseMessage(
                    StatusCode));
        }
    }
}
