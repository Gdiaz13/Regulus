namespace api.Services;

// Shared prediction-scoring math so the live accuracy reads and the persisted
// scoring job always agree on how a prediction is judged.
public static class AccuracyMath
{
    public static double PercentChange(decimal start, decimal finish)
    {
        return start == 0 ? 0 : Math.Round((double)((finish - start) / start * 100), 2);
    }

    public static bool DirectionMatched(double predictedPercent, double actualPercent)
    {
        return predictedPercent.CompareTo(0) == actualPercent.CompareTo(0);
    }

    public static double AbsoluteError(double predictedPercent, double actualPercent)
    {
        return Math.Abs(actualPercent - predictedPercent);
    }

    public static DateOnly TargetDate(DateTime predictedOn, int timeHorizonDays)
    {
        return DateOnly.FromDateTime(predictedOn.Date).AddDays(timeHorizonDays);
    }
}
