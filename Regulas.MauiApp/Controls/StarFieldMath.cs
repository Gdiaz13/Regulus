namespace Regulas.MauiApp.Controls;

// Pure star maths, kept out of the drawing code so it can be unit tested.
public static class StarFieldMath
{
    public const double BrightestMagnitude = 1.4;
    public const double FaintestMagnitude = 5.5;
    private const double TwinkleDepth = 0.22;
    private const double TwinkleSpeed = 1.4;

    // Astronomical magnitude runs backwards: smaller number, brighter star.
    public static double Brightness(double magnitude)
    {
        var span = FaintestMagnitude - BrightestMagnitude;
        return Clamp((FaintestMagnitude - magnitude) / span, 0.08, 1.0);
    }

    // Stars never blink out; they breathe around their own brightness.
    public static double Twinkle(double brightness, double phase, double seconds)
    {
        var wave = Math.Sin(phase + seconds * TwinkleSpeed);
        return Clamp(brightness * (1 - TwinkleDepth + TwinkleDepth * wave), 0.0, 1.0);
    }

    public static double Radius(double brightness, double scale)
    {
        return (0.6 + brightness * brightness * 2.6) * scale;
    }

    // Deterministic field so the same sky redraws identically every frame.
    public static IReadOnlyList<SkyStar> Field(int count, uint seed)
    {
        var state = seed == 0 ? 1u : seed;
        var stars = new List<SkyStar>(count);
        for (var index = 0; index < count; index++)
        {
            stars.Add(FieldStar(ref state));
        }
        return stars;
    }

    private static SkyStar FieldStar(ref uint state)
    {
        var x = Unit(ref state);
        var y = Unit(ref state);
        var magnitude = 3.2 + Unit(ref state) * (FaintestMagnitude - 3.2);
        return new SkyStar(x, y, magnitude, Unit(ref state) * Math.Tau);
    }

    private static double Unit(ref uint state)
    {
        state = unchecked(state * 1664525u + 1013904223u);
        return state / (double)uint.MaxValue;
    }

    public static double Clamp(double value, double min, double max)
    {
        return Math.Min(Math.Max(value, min), max);
    }
}
