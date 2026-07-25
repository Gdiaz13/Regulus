namespace Regulas.MauiApp.Controls;

// One star in the hero sky. X/Y are normalized 0..1 so the field scales with
// whatever size the view gets. Magnitude is the astronomical scale, where
// lower is brighter (Regulus is 1.4, the faintest naked-eye stars are ~6).
public sealed record SkyStar(
    double X,
    double Y,
    double Magnitude,
    double Phase,
    string Name = ""
)
{
    public bool IsNamed => !string.IsNullOrEmpty(Name);
}
