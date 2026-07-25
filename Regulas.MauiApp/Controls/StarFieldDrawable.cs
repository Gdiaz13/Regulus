namespace Regulas.MauiApp.Controls;

// Paints the Leo sky: a deterministic background field, the constellation
// figure, and Regulus itself carrying the brand accent. All colours come from
// the shared design tokens via StarFieldView, so the sky stays on-brand.
public sealed class StarFieldDrawable : IDrawable
{
    private const int FieldCount = 120;
    private readonly IReadOnlyList<SkyStar> _field = StarFieldMath.Field(FieldCount, 2026_07_24);

    public double Seconds { get; set; }
    public int? SelectedIndex { get; set; }
    public Color SkyTop { get; set; } = Color.FromArgb("#05080F");
    public Color SkyBottom { get; set; } = Color.FromArgb("#0B111E");
    public Color StarColor { get; set; } = Color.FromArgb("#E1E7EF");
    public Color LinkColor { get; set; } = Color.FromArgb("#4C88FF");
    public Color RegulusColor { get; set; } = Color.FromArgb("#FFCC00");

    public void Draw(ICanvas canvas, RectF rect)
    {
        var figure = SkyLayout.Figure(rect);
        DrawSky(canvas, rect);
        DrawField(canvas, rect);
        DrawLinks(canvas, figure);
        DrawConstellation(canvas, figure);
        canvas.Alpha = 1f;
    }

    private void DrawSky(ICanvas canvas, RectF rect)
    {
        var paint = new LinearGradientPaint
        {
            StartColor = SkyTop,
            EndColor = SkyBottom,
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0.35, 1),
        };
        canvas.SetFillPaint(paint, rect);
        canvas.FillRectangle(rect);
    }

    private void DrawField(ICanvas canvas, RectF rect)
    {
        var scale = Scale(rect);
        canvas.FillColor = StarColor;
        foreach (var star in _field)
        {
            DrawFieldStar(canvas, rect, star, scale);
        }
    }

    private void DrawFieldStar(ICanvas canvas, RectF rect, SkyStar star, float scale)
    {
        var brightness = StarFieldMath.Twinkle(StarFieldMath.Brightness(star.Magnitude), star.Phase, Seconds);
        canvas.Alpha = (float)(brightness * 0.7);
        canvas.FillCircle(X(rect, star), Y(rect, star), (float)StarFieldMath.Radius(brightness, scale * 0.7));
    }

    // Faint joins: the figure should read as a suggestion, not a diagram.
    private void DrawLinks(ICanvas canvas, RectF rect)
    {
        canvas.StrokeColor = LinkColor;
        canvas.StrokeSize = 1f;
        canvas.Alpha = 0.28f;
        foreach (var (from, to) in LeoConstellation.Links)
        {
            DrawLink(canvas, rect, LeoConstellation.Stars[from], LeoConstellation.Stars[to]);
        }
    }

    private static void DrawLink(ICanvas canvas, RectF rect, SkyStar from, SkyStar to)
    {
        canvas.DrawLine(X(rect, from), Y(rect, from), X(rect, to), Y(rect, to));
    }

    private void DrawConstellation(ICanvas canvas, RectF rect)
    {
        var scale = Scale(rect);
        for (var index = 0; index < LeoConstellation.Stars.Count; index++)
        {
            DrawNamedStar(canvas, rect, index, scale);
        }
    }

    private void DrawNamedStar(ICanvas canvas, RectF rect, int index, float scale)
    {
        var star = LeoConstellation.Stars[index];
        var isRegulus = index == LeoConstellation.RegulusIndex;
        var brightness = StarFieldMath.Twinkle(StarFieldMath.Brightness(star.Magnitude), star.Phase, Seconds);
        var radius = (float)StarFieldMath.Radius(brightness, scale * 1.6);
        DrawGlow(canvas, rect, star, radius, isRegulus ? RegulusColor : StarColor);
        DrawCore(canvas, rect, star, radius, brightness, isRegulus);
        DrawSelection(canvas, rect, star, radius, index);
    }

    private static void DrawGlow(ICanvas canvas, RectF rect, SkyStar star, float radius, Color color)
    {
        canvas.FillColor = color;
        canvas.Alpha = 0.10f;
        canvas.FillCircle(X(rect, star), Y(rect, star), radius * 3.4f);
        canvas.Alpha = 0.18f;
        canvas.FillCircle(X(rect, star), Y(rect, star), radius * 1.9f);
    }

    private void DrawCore(ICanvas canvas, RectF rect, SkyStar star, float radius, double brightness, bool isRegulus)
    {
        canvas.FillColor = isRegulus ? RegulusColor : StarColor;
        canvas.Alpha = (float)StarFieldMath.Clamp(brightness + 0.15, 0, 1);
        canvas.FillCircle(X(rect, star), Y(rect, star), radius);
        if (isRegulus)
        {
            DrawSpikes(canvas, rect, star, radius);
        }
    }

    // Only Regulus gets diffraction spikes; it is the star the app is named for.
    private void DrawSpikes(ICanvas canvas, RectF rect, SkyStar star, float radius)
    {
        var length = radius * 4.5f;
        canvas.StrokeColor = RegulusColor;
        canvas.StrokeSize = 1f;
        canvas.Alpha = 0.55f;
        canvas.DrawLine(X(rect, star) - length, Y(rect, star), X(rect, star) + length, Y(rect, star));
        canvas.DrawLine(X(rect, star), Y(rect, star) - length, X(rect, star), Y(rect, star) + length);
    }

    private void DrawSelection(ICanvas canvas, RectF rect, SkyStar star, float radius, int index)
    {
        if (SelectedIndex != index)
        {
            return;
        }
        canvas.StrokeColor = RegulusColor;
        canvas.StrokeSize = 1.5f;
        canvas.Alpha = 0.9f;
        canvas.DrawCircle(X(rect, star), Y(rect, star), radius * 3.2f);
    }

    private static float Scale(RectF rect)
    {
        return Math.Max(1f, Math.Min(rect.Width, rect.Height) / 220f);
    }

    private static float X(RectF rect, SkyStar star)
    {
        return rect.X + (float)star.X * rect.Width;
    }

    private static float Y(RectF rect, SkyStar star)
    {
        return rect.Y + (float)star.Y * rect.Height;
    }
}
