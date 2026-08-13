namespace Regulas.MauiApp.Models;

// A holding drawn as a star. Magnitude runs 0..1, brightest first, so the
// portfolio reads like a sky: the biggest companies shine hardest.
public sealed record PortfolioStarRow(PortfolioStock Stock, double Magnitude)
{
    public int Id => Stock.Id;
    public string Symbol => Stock.Symbol;
    public string CompanyName => Stock.CompanyName;
    public decimal PurchasePrice => Stock.PurchasePrice;
    public string Industry => Stock.Industry;
    public double StarSize => 10 + (Magnitude * 16);
    public double StarOpacity => 0.35 + (Magnitude * 0.65);
}
