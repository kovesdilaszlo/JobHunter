
using System.Net;
using System.Text;
using System.Text.Json;
using JobHunter.Application.DTOs;
using JobHunter.Domain.Enums;
using JobHunter.Infrastructure.Providers.Jooble;
using Microsoft.Extensions.Options;
using Xunit;

namespace JobHunter.Test.Infrastructure.Providers;

public class JoobleJobProviderTests
{
    private static JobProviderSearchRequest CreateRequest(
        int maxResults = 100)
    {
        return new JobProviderSearchRequest(
            "Junior .NET Developer",
            "Budapest",
            true,
            maxResults);
    }

    private static JoobleOptions CreateOptions()
    {
        return new JoobleOptions
        {
            ApiKey = "test-api-key",
            PageSize = 20,
            MaxPages = 5,
            Radius = 26,
            TimeoutSeconds = 30
        };
    }

    private static JoobleJobProvider CreateProvider(
        HttpClient client,
        JoobleOptions? options = null)
    {
        return new JoobleJobProvider(
            client,
            Options.Create(
                options ?? CreateOptions()));
    }

    private static HttpClient CreateClient(
        HttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress =
                new Uri("https://hu.jooble.org/"),

            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    private static string CreateResponse(
        int totalCount,
        int jobCount,
        int firstId = 1)
    {
        var jobs = Enumerable
            .Range(firstId, jobCount)
            .Select(id => new
            {
                id,
                title = $"Developer {id}",
                company = "Example Company",
                location = "Budapest",
                snippet = "ASP.NET Core position",
                salary = "800000 HUF",
                source = "example.com",
                type = "Full-time",
                link = $"https://example.com/jobs/{id}",
                updated = "2026-11-01T10:00:00Z"
            })
            .ToArray();

        return JsonSerializer.Serialize(
            new
            {
                totalCount,
                jobs
            });
    }

    [Fact]
    public void SourceType_ReturnsJooble()
    {
        using var client = CreateClient(
            new FakeHttpMessageHandler());

        var provider = CreateProvider(client);

        Assert.Equal(
            JobSourceType.Jooble,
            provider.SourceType);

        Assert.Equal("Jooble", provider.Name);
    }

    [Fact]
    public async Task SearchAsync_ValidResponse_ReturnsJobs()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueJson(
            CreateResponse(2, 2));

        using var client = CreateClient(handler);

        var provider = CreateProvider(client);

        var result = await provider.SearchAsync(
            CreateRequest());

        Assert.Equal(2, result.Count);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task SearchAsync_MapsResponseCorrectly()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueJson(
            CreateResponse(1, 1, 123));

        using var client = CreateClient(handler);

        var provider = CreateProvider(client);

        var result = await provider.SearchAsync(
            CreateRequest());

        var job = Assert.Single(result);

        Assert.Equal("Developer 123", job.Title);

        Assert.Equal(
            "Example Company",
            job.Company);

        Assert.Equal("Budapest", job.Location);

        Assert.Equal(
            "ASP.NET Core position",
            job.Description);

        Assert.Equal("123", job.ExternalId);

        Assert.Equal(
            "https://example.com/jobs/123",
            job.SourceUrl);

        Assert.Equal(
            WorkArrangement.Unknown,
            job.WorkArrangement);
    }

    [Fact]
    public async Task SearchAsync_SendsCorrectHttpRequest()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueJson(
            CreateResponse(0, 0));

        using var client = CreateClient(handler);

        var provider = CreateProvider(client);

        await provider.SearchAsync(CreateRequest());

        var request = Assert.Single(handler.Requests);

        Assert.Equal(HttpMethod.Post, request.Method);

        Assert.Equal(
            "/api/test-api-key",
            request.Path);

        using var document =
            JsonDocument.Parse(request.Body);

        var root = document.RootElement;

        Assert.Equal(
            "Junior .NET Developer",
            root.GetProperty("keywords").GetString());

        Assert.Equal(
            "Budapest",
            root.GetProperty("location").GetString());

        Assert.Equal(
            "26",
            root.GetProperty("radius").GetString());

        Assert.Equal(
            1,
            root.GetProperty("page").GetInt32());

        Assert.Equal(
            20,
            root.GetProperty("ResultOnPage").GetInt32());

        Assert.False(
            root.GetProperty("companysearch").GetBoolean());
    }

    [Fact]
    public async Task SearchAsync_MultiplePages_ReturnsAllJobs()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueJson(
            CreateResponse(25, 20, 1));

        handler.EnqueueJson(
            CreateResponse(25, 5, 21));

        using var client = CreateClient(handler);

        var provider = CreateProvider(client);

        var result = await provider.SearchAsync(
            CreateRequest());

        Assert.Equal(25, result.Count);

        Assert.Equal(2, handler.RequestCount);

        using var document = JsonDocument.Parse(
            handler.Requests[1].Body);

        Assert.Equal(
            2,
            document.RootElement
                .GetProperty("page")
                .GetInt32());
    }

    [Fact]
    public async Task SearchAsync_MaxResults_LimitsResults()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueJson(
            CreateResponse(100, 20));

        using var client = CreateClient(handler);

        var provider = CreateProvider(client);

        var result = await provider.SearchAsync(
            CreateRequest(10));

        Assert.Equal(10, result.Count);

        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task SearchAsync_MaxPages_LimitsRequests()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueJson(
            CreateResponse(100, 20, 1));

        handler.EnqueueJson(
            CreateResponse(100, 20, 21));

        using var client = CreateClient(handler);

        var options = CreateOptions();

        options.MaxPages = 2;

        var provider = CreateProvider(
            client,
            options);

        var result = await provider.SearchAsync(
            CreateRequest());

        Assert.Equal(40, result.Count);

        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task SearchAsync_EmptyResponse_ReturnsEmptyList()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueJson(
            CreateResponse(0, 0));

        using var client = CreateClient(handler);

        var provider = CreateProvider(client);

        var result = await provider.SearchAsync(
            CreateRequest());

        Assert.Empty(result);

        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task SearchAsync_MissingApiKey_ThrowsException()
    {
        var handler = new FakeHttpMessageHandler();

        using var client = CreateClient(handler);

        var options = CreateOptions();

        options.ApiKey = "";

        var provider = CreateProvider(
            client,
            options);

        await Assert.ThrowsAsync<
            InvalidOperationException>(
                () => provider.SearchAsync(
                    CreateRequest()));

        Assert.Equal(0, handler.RequestCount);
    }

    [Theory]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(429)]
    [InlineData(500)]
    public async Task SearchAsync_HttpError_ThrowsApiException(
        int statusCode)
    {
        var handler = new FakeHttpMessageHandler();

        handler.Enqueue(
            new HttpResponseMessage(
                (HttpStatusCode)statusCode));

        using var client = CreateClient(handler);

        var provider = CreateProvider(client);

        var exception =
            await Assert.ThrowsAsync<JoobleApiException>(
                () => provider.SearchAsync(
                    CreateRequest()));

        Assert.Equal(
            (HttpStatusCode)statusCode,
            exception.StatusCode);

        Assert.DoesNotContain(
            "test-api-key",
            exception.Message);
    }

    [Fact]
    public async Task SearchAsync_InvalidJson_ThrowsApiException()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueJson("invalid-json");

        using var client = CreateClient(handler);

        var provider = CreateProvider(client);

        await Assert.ThrowsAsync<JoobleApiException>(
            () => provider.SearchAsync(
                CreateRequest()));
    }

    [Fact]
    public async Task SearchAsync_MissingJobs_ThrowsApiException()
    {
        var handler = new FakeHttpMessageHandler();

        handler.EnqueueJson(
            "{\"totalCount\":1}");

        using var client = CreateClient(handler);

        var provider = CreateProvider(client);

        await Assert.ThrowsAsync<JoobleApiException>(
            () => provider.SearchAsync(
                CreateRequest()));
    }

    [Fact]
    public async Task SearchAsync_CancelledToken_ThrowsException()
    {
        var handler = new FakeHttpMessageHandler();

        using var client = CreateClient(handler);

        var provider = CreateProvider(client);

        using var cancellation =
            new CancellationTokenSource();

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<
            OperationCanceledException>(
                () => provider.SearchAsync(
                    CreateRequest(),
                    cancellation.Token));

        Assert.Equal(0, handler.RequestCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchAsync_InvalidMaxResults_ThrowsException(
        int maxResults)
    {
        using var client = CreateClient(
            new FakeHttpMessageHandler());

        var provider = CreateProvider(client);

        await Assert.ThrowsAsync<
            ArgumentOutOfRangeException>(
                () => provider.SearchAsync(
                    CreateRequest(maxResults)));
    }

    [Fact]
    public async Task SearchAsync_EmptyKeywords_ThrowsException()
    {
        using var client = CreateClient(
            new FakeHttpMessageHandler());

        var provider = CreateProvider(client);

        var request = CreateRequest() with
        {
            Keywords = ""
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => provider.SearchAsync(request));
    }

    [Fact]
    public async Task SearchAsync_EmptyLocation_ThrowsException()
    {
        using var client = CreateClient(
            new FakeHttpMessageHandler());

        var provider = CreateProvider(client);

        var request = CreateRequest() with
        {
            Location = ""
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => provider.SearchAsync(request));
    }

    private sealed record CapturedRequest(
        HttpMethod Method,
        string Path,
        string Body);

    private sealed class FakeHttpMessageHandler
        : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage>
            _responses = new();

        public List<CapturedRequest> Requests
        {
            get;
        } = new();

        public int RequestCount =>
            Requests.Count;

        public void Enqueue(
            HttpResponseMessage response)
        {
            _responses.Enqueue(response);
        }

        public void EnqueueJson(string json)
        {
            Enqueue(
                new HttpResponseMessage(
                    HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json")
                });
        }

        protected override async Task<HttpResponseMessage>
            SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(
                    cancellationToken);

            Requests.Add(
                new CapturedRequest(
                    request.Method,
                    request.RequestUri!.AbsolutePath,
                    body));

            if (_responses.Count == 0)
            {
                throw new InvalidOperationException(
                    "No fake HTTP response configured.");
            }

            return _responses.Dequeue();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (_responses.Count > 0)
                {
                    _responses.Dequeue().Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
