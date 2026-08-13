using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using api.Contracts;
using api.Services;
using Xunit;

namespace api.Tests;

public class StockTechAiClientTests
{
    [Fact]
    public async Task Train_posts_series_and_reads_real_metrics()
    {
        var handler = new CaptureHandler(TrainResponseJson);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:8101/") };
        var request = new AiTrainRequest([new AiTrainSeries("AMD", [100m, 105m, 110m])]);

        var response = await new StockTechAiClient(http).TrainAsync(request, CancellationToken.None);

        Assert.Equal("/train", handler.Request?.RequestUri?.AbsolutePath);
        Assert.Contains("\"symbol\":\"AMD\"", handler.Body);
        Assert.True(response.Trained);
        Assert.Equal(0.42, response.Metrics?.GetProperty("testMae").GetDouble());
    }

    [Fact]
    public async Task Train_throws_on_non_success_response()
    {
        var http = new HttpClient(StubHttpMessageHandler.Status(HttpStatusCode.BadGateway))
        {
            BaseAddress = new Uri("http://localhost:8101/"),
        };

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            new StockTechAiClient(http).TrainAsync(new AiTrainRequest([]), CancellationToken.None)
        );
    }

    [Fact]
    public async Task Train_rejects_a_response_larger_than_the_contract_limit()
    {
        var padding = new string('x', (int)StockTechAiClient.MaxResponseBytes + 1);
        var http = new HttpClient(StubHttpMessageHandler.Json($"{{\"padding\":\"{padding}\"}}"))
        {
            BaseAddress = new Uri("http://localhost:8101/"),
        };

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            new StockTechAiClient(http).TrainAsync(new AiTrainRequest([]), CancellationToken.None));
    }

    [Fact]
    public async Task Train_timeout_covers_a_stalled_response_body()
    {
        using var callerTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var http = new HttpClient(new StalledBodyHandler())
        {
            BaseAddress = new Uri("http://localhost:8101/"),
            Timeout = TimeSpan.FromMilliseconds(50),
        };
        var stopwatch = Stopwatch.StartNew();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new StockTechAiClient(http).TrainAsync(new AiTrainRequest([]), callerTimeout.Token));

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1));
    }

    [Theory]
    [MemberData(nameof(OmittedResponseFields))]
    public async Task Train_rejects_a_response_with_an_omitted_required_field(string json)
    {
        var http = new HttpClient(StubHttpMessageHandler.Json(json))
        {
            BaseAddress = new Uri("http://localhost:8101/"),
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new StockTechAiClient(http).TrainAsync(new AiTrainRequest([]), CancellationToken.None));
    }

    private sealed class CaptureHandler(string json) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string Body { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Request = request;
            Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(token);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };
        }
    }

    private sealed class StalledBodyHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken token)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StalledContent(),
            });
        }
    }

    private sealed class StalledContent : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return Task.Delay(Timeout.InfiniteTimeSpan);
        }

        protected override Task SerializeToStreamAsync(
            Stream stream, TransportContext? context, CancellationToken token)
        {
            return Task.Delay(Timeout.InfiniteTimeSpan, token);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    private const string TrainResponseJson = """
        {"status":"completed","modelName":"StockTechAI","modelVersion":"0.2.0","message":"trained","isMock":false,"contractVersion":"1.0","trained":true,"artifact":{"damping":0.5},"metrics":{"testMae":0.42,"baselineMae":0.5,"improved":true},"warnings":[]}
        """;

    private const string InsufficientResponseJson = """
        {"status":"insufficient-data","modelName":"StockTechAI","modelVersion":"0.2.0","message":"not enough data","isMock":false,"contractVersion":"1.0","trained":false,"artifact":null,"metrics":null,"warnings":[]}
        """;

    public static TheoryData<string> OmittedResponseFields => new()
    {
        TrainResponseJson.Replace("\"isMock\":false,", string.Empty),
        TrainResponseJson.Replace("\"message\":\"trained\",", string.Empty),
        TrainResponseJson.Replace(",\"warnings\":[]", string.Empty),
        InsufficientResponseJson.Replace("\"isMock\":false,", string.Empty),
        InsufficientResponseJson.Replace("\"trained\":false,", string.Empty),
        InsufficientResponseJson.Replace("\"artifact\":null,", string.Empty),
        InsufficientResponseJson.Replace("\"metrics\":null,", string.Empty),
        InsufficientResponseJson.Replace("\"message\":\"not enough data\",", string.Empty),
        InsufficientResponseJson.Replace(",\"warnings\":[]", string.Empty),
    };
}
