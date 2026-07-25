namespace Regulas.MauiApp.Controls;

// Where Leo sits inside the view. Drawing and tapping must share this box or
// taps land on stars that are not where they look; keeping the transform in one
// place is what stops them drifting apart.
public static class SkyLayout
{
    // Leo is a wide figure. Stretching it to a banner stops it reading as Leo.
    private const float Aspect = 1.7f;
    private const float HeightShare = 0.9f;
    // Given a whole window the figure would swell until it swallowed the
    // content on top of it, so it stops growing and stays a motif.
    private const float MaxHeight = 340f;
    // Pushed right of centre so hero text on the left never crosses the figure.
    // Pages with a centred card push it further still, or Leo hides behind them.
    public const float DefaultBias = 0.62f;

    public static RectF Figure(RectF rect, float bias = DefaultBias)
    {
        var height = Math.Min(Math.Min(rect.Height * HeightShare, rect.Width / Aspect), MaxHeight);
        var width = height * Aspect;
        return new RectF(
            rect.X + (rect.Width - width) * Math.Clamp(bias, 0f, 1f),
            rect.Y + (rect.Height - height) * 0.5f,
            width,
            height);
    }

    // Screen point back to constellation space, so a tap can be matched to a star.
    public static (double X, double Y) Normalize(RectF figure, double x, double y)
    {
        if (figure.Width <= 0 || figure.Height <= 0)
        {
            return (double.NaN, double.NaN);
        }
        return ((x - figure.X) / figure.Width, (y - figure.Y) / figure.Height);
    }
}
