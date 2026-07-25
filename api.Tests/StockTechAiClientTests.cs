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

    private const string TrainResponseJson = """
        {"status":"completed","modelName":"StockTechAI","modelVersion":"0.2.0","message":"trained","isMock":false,"contractVersion":"1.0","trained":true,"artifact":{"damping":0.5},"metrics":{"testMae":0.42,"baselineMae":0.5,"improved":true},"warnings":[]}
        """;
}
