using api.Contracts;
using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

// Scores saved predictions against stored prices. It is computed for now, so
// the app can start measuring model quality before adding accuracy tables.
public static class PredictionAccuracyStore
{
    public static async Task<List<PredictionAccuracyResponse>> ListAsync(
        ApplicationDBContext db,
        string? assetId,
        int? take
    )
    {
        var predictions = await LoadPredictions(db, CleanAssetId(assetId), ClampTake(take));
        var prices = await LoadPrices(db);
        return predictions.Select(prediction => Score(prediction, prices)).OfType<PredictionAccuracyResponse>().ToList();
    }

    private static Task<List<Prediction>> LoadPredictions(ApplicationDBContext db, string assetId, int take)
    {
        var query = db.Predictions.AsQueryable();
        if (!string.IsNullOrWhiteSpace(assetId))
        {
            query = query.Where(prediction => prediction.AssetId == assetId);
        }
        return query.OrderByDescending(prediction => prediction.CreatedOn).Take(take).ToListAsync();
    }

    private static Task<List<PriceHistory>> LoadPrices(ApplicationDBContext db)
    {
        return db.PriceHistories.Include(price => price.Asset).ToListAsync();
    }

    private static PredictionAccuracyResponse? Score(Prediction prediction, List<PriceHistory> prices)
    {
        var targetDate = TargetDate(prediction);
        var actual = ActualPrice(prediction, prices, targetDate);
        return actual is null ? null : Response(prediction, actual, targetDate);
    }

    private static PriceHistory? ActualPrice(Prediction prediction, List<PriceHistory> prices, DateOnly targetDate)
    {
        return prices
            .Where(price => MatchesPrediction(price, prediction) && price.Date >= targetDate)
            .OrderBy(price => price.Date)
            .FirstOrDefault();
    }

    private static PredictionAccuracyResponse Response(
        Prediction prediction,
        PriceHistory actual,
        DateOnly targetDate
    )
    {
        var actualPercent = PercentChange(prediction.CurrentPrice, actual.Close);
        var error = Math.Abs(actualPercent - prediction.PredictedPercentChange);
        return BuildResponse(prediction, actual, targetDate, actualPercent, error);
    }

    private static PredictionAccuracyResponse BuildResponse(Prediction prediction, PriceHistory actual, DateOnly targetDate, double actualPercent, double error)
    {
        return new PredictionAccuracyResponse(
            prediction.Id, prediction.AssetId, prediction.AssetName, prediction.AssetType.ToString(),
            prediction.ModelName, prediction.ModelVersion, prediction.CurrentPrice, prediction.PredictedPrice,
            actual.Close, prediction.PredictedPercentChange, actualPercent, error,
            Direction(prediction.PredictedPercentChange) == Direction(actualPercent),
            prediction.TimeHorizonDays, prediction.CreatedOn, targetDate, actual.Date, prediction.IsMock
        );
    }

    private static bool MatchesPrediction(PriceHistory price, Prediction prediction)
    {
        return price.Asset?.Symbol == prediction.AssetId && price.Asset.AssetType == prediction.AssetType;
    }

    private static DateOnly TargetDate(Prediction prediction)
    {
        return DateOnly.FromDateTime(prediction.CreatedOn.Date).AddDays(prediction.TimeHorizonDays);
    }

    private static double PercentChange(decimal start, decimal finish)
    {
        return start == 0 ? 0 : Math.Round((double)((finish - start) / start * 100), 2);
    }

    private static int Direction(double percent)
    {
        return percent.CompareTo(0);
    }

    private static string CleanAssetId(string? assetId)
    {
        return assetId?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private static int ClampTake(int? take)
    {
        return Math.Clamp(take ?? 25, 1, 100);
    }
}
