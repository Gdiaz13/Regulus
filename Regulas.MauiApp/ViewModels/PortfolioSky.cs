using Regulas.MauiApp.Controls;
using Regulas.MauiApp.Models;

namespace Regulas.MauiApp.ViewModels;

// Ranks holdings by market cap so the portfolio list can be read as a sky.
// Market caps span orders of magnitude, so the scale is logarithmic - which is
// also how real stellar magnitude works.
public static class PortfolioSky
{
    private const double FaintestMagnitude = 0.2;

    public static IReadOnlyList<PortfolioStarRow> Rank(IReadOnlyList<PortfolioStock> stocks)
    {
        var scales = stocks.Select(Scale).ToList();
        var smallest = scales.Count == 0 ? 0 : scales.Min();
        var largest = scales.Count == 0 ? 0 : scales.Max();
        return [.. stocks.Select((stock, index) => Row(stock, scales[index], smallest, largest))];
    }

    private static PortfolioStarRow Row(PortfolioStock stock, double scale, double smallest, double largest)
    {
        return new PortfolioStarRow(stock, Magnitude(scale, smallest, largest));
    }

    // A missing or zero market cap still gets a visible, faint star.
    private static double Scale(PortfolioStock stock)
    {
        return Math.Log10(Math.Max(1, stock.MarketCap));
    }

    private static double Magnitude(double scale, double smallest, double largest)
    {
        var span = largest - smallest;
        return span <= 0 ? 1.0 : StarFieldMath.Clamp((scale - smallest) / span, FaintestMagnitude, 1.0);
    }
}
