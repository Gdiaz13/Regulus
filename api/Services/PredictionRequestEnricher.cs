using System.Data.Common;
using api.Contracts;
using api.Models;

namespace api.Services;

// Attaches stored closes to predict requests so real specialist models have
// actual price data to work with. Assets without history pass through
// unchanged and the specialists fall back to their clearly-marked mocks.
public sealed class PredictionRequestEnricher
{
    private const int ClosesToAttach = 30;
    private readonly PriceHistoryStore _store;

    public PredictionRequestEnricher(PriceHistoryStore store)
    {
        _store = store;
    }

    public async Task<List<AiPredictRequest>> EnrichAsync(List<AiPredictRequest> requests)
    {
        var enriched = new List<AiPredictRequest>(requests.Count);
        foreach (var request in requests)
        {
            enriched.Add(await EnrichOneAsync(request));
        }
        return enriched;
    }

    // Enrichment is best-effort: a database problem must not block predictions.
    private async Task<AiPredictRequest> EnrichOneAsync(AiPredictRequest request)
    {
        try
        {
            var closes = await ClosesAsync(request);
            return closes.Count == 0 ? request : request with { RecentCloses = closes };
        }
        catch (Exception exception) when (IsDatabaseException(exception))
        {
            return request;
        }
    }

    private async Task<List<decimal>> ClosesAsync(AiPredictRequest request)
    {
        var points = await _store.ListPointsAsync(request.AssetId, ParseType(request.AssetType), ClosesToAttach);
        return points.Select(point => point.Close).ToList();
    }

    private static AssetType ParseType(string value)
    {
        return Enum.TryParse<AssetType>(value, ignoreCase: true, out var parsed) ? parsed : AssetType.Stock;
    }

    private static bool IsDatabaseException(Exception exception)
    {
        return exception is DbException or InvalidOperationException;
    }
}
