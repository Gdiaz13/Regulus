namespace api.Services;

// Keeps price-history chart reads bounded so the API never loads an entire
// history table for a normal screen request.
public static class PriceHistoryQueryOptions
{
    public const int DefaultTake = 365;
    public const int MaxTake = 1000;

    public static int NormalizeTake(int? take)
    {
        return Math.Clamp(take ?? DefaultTake, 1, MaxTake);
    }
}
