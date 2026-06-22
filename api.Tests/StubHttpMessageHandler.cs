using System.Net;
using System.Text;

namespace api.Tests;

// A fake HttpMessageHandler that returns a canned response or throws, so the
// RegulasAiClient can be exercised without a live AI service.
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    private StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_responder(request));
    }

    public static StubHttpMessageHandler Json(string json)
    {
        return new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });
    }

    public static StubHttpMessageHandler Status(HttpStatusCode code)
    {
        return new StubHttpMessageHandler(_ => new HttpResponseMessage(code));
    }

    public static StubHttpMessageHandler Throws()
    {
        return new StubHttpMessageHandler(_ => throw new HttpRequestException("unreachable"));
    }
}
