namespace Regulas.MauiApp.Controls;

// The real constellation the app is named after. Regulus is Alpha Leonis, the
// brightest star in Leo, so the hero draws the actual figure with real
// magnitudes rather than decorative sparkles. Positions are chart-normalized.
public static class LeoConstellation
{
    public const int RegulusIndex = 0;

    public static IReadOnlyList<SkyStar> Stars { get; } =
    [
        new(0.700, 0.660, 1.40, 0.0, "Regulus"),
        new(0.715, 0.545, 3.51, 1.1, "Eta Leonis"),
        new(0.735, 0.425, 2.08, 2.3, "Algieba"),
        new(0.715, 0.315, 3.44, 3.0, "Adhafera"),
        new(0.655, 0.245, 3.88, 4.2, "Rasalas"),
        new(0.575, 0.275, 2.98, 5.1, "Ras Elased"),
        new(0.335, 0.335, 2.56, 0.7, "Zosma"),
        new(0.355, 0.535, 3.33, 2.9, "Chertan"),
        new(0.175, 0.395, 2.14, 4.6, "Denebola"),
    ];

    // The Sickle first (Regulus up through the lion's head), then the body.
    public static IReadOnlyList<(int From, int To)> Links { get; } =
    [
        (5, 4), (4, 3), (3, 2), (2, 1), (1, 0),
        (2, 6), (6, 8), (8, 7), (7, 0), (6, 7),
    ];

    public static SkyStar Regulus => Stars[RegulusIndex];

    // Nearest star to a tap, in normalized space, or null when the tap missed.
    public static int? NearestIndex(double x, double y, double maxDistance)
    {
        var best = -1;
        var bestDistance = maxDistance;
        for (var index = 0; index < Stars.Count; index++)
        {
            bestDistance = Closer(x, y, index, bestDistance, ref best);
        }
        return best < 0 ? null : best;
    }

    private static double Closer(double x, double y, int index, double bestDistance, ref int best)
    {
        var distance = Distance(x, y, Stars[index]);
        if (distance >= bestDistance)
        {
            return bestDistance;
        }
        best = index;
        return distance;
    }

    private static double Distance(double x, double y, SkyStar star)
    {
        return Math.Sqrt(Math.Pow(x - star.X, 2) + Math.Pow(y - star.Y, 2));
    }
}
