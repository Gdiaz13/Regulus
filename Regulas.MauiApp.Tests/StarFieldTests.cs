using Regulas.MauiApp.Controls;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.ViewModels;
using Xunit;

namespace Regulas.MauiApp.Tests;

public class StarFieldTests
{
    [Fact]
    public void Brighter_stars_have_lower_magnitudes()
    {
        var regulus = StarFieldMath.Brightness(LeoConstellation.Regulus.Magnitude);
        var faint = StarFieldMath.Brightness(4.8);

        Assert.Equal(1.0, regulus, 3);
        Assert.True(faint < regulus);
        Assert.True(faint > 0);
    }

    [Fact]
    public void Twinkling_breathes_without_ever_blinking_out()
    {
        var brightness = StarFieldMath.Brightness(2.0);
        var samples = Enumerable.Range(0, 40).Select(step => StarFieldMath.Twinkle(brightness, 0.4, step * 0.1)).ToList();

        Assert.All(samples, value => Assert.InRange(value, brightness * 0.5, brightness));
        Assert.True(samples.Max() - samples.Min() > 0.05);
    }

    [Fact]
    public void The_field_is_deterministic_so_frames_do_not_jitter()
    {
        var first = StarFieldMath.Field(60, 99);
        var second = StarFieldMath.Field(60, 99);

        Assert.Equal(60, first.Count);
        Assert.Equal(first, second);
        Assert.All(first, star => Assert.InRange(star.X, 0, 1));
        Assert.All(first, star => Assert.InRange(star.Y, 0, 1));
    }

    [Fact]
    public void Leo_is_a_joined_figure_with_regulus_as_its_brightest_star()
    {
        var brightest = LeoConstellation.Stars.MinBy(star => star.Magnitude);

        Assert.Equal("Regulus", brightest!.Name);
        Assert.Equal("Regulus", LeoConstellation.Regulus.Name);
        Assert.All(LeoConstellation.Stars, star => Assert.True(star.IsNamed));
        Assert.All(LeoConstellation.Links, link => Assert.NotEqual(link.From, link.To));
    }

    [Fact]
    public void Every_leo_star_is_reachable_through_the_figure()
    {
        var linked = LeoConstellation.Links.SelectMany(link => new[] { link.From, link.To }).Distinct();

        Assert.Equal(LeoConstellation.Stars.Count, linked.Count());
    }

    [Fact]
    public void Tapping_near_a_star_selects_it_and_missing_selects_nothing()
    {
        var regulus = LeoConstellation.Regulus;

        Assert.Equal(LeoConstellation.RegulusIndex, LeoConstellation.NearestIndex(regulus.X + 0.01, regulus.Y, 0.07));
        Assert.Null(LeoConstellation.NearestIndex(0.02, 0.95, 0.07));
    }

    [Fact]
    public void Leo_keeps_its_shape_in_a_wide_banner()
    {
        var figure = SkyLayout.Figure(new RectF(0, 0, 1900, 300));

        Assert.Equal(1.7, figure.Width / figure.Height, 2);
        Assert.True(figure.Height <= 300);
        Assert.True(figure.X > 0 && figure.Right < 1900);
    }

    [Fact]
    public void Leo_stops_growing_so_it_never_swallows_the_page()
    {
        var wide = SkyLayout.Figure(new RectF(0, 0, 1920, 1023));

        Assert.True(wide.Height <= 340);
        Assert.Equal(1.7, wide.Width / wide.Height, 2);
        Assert.True(wide.Height < 1023 * 0.9);
    }

    // The bug this guards: the figure was drawn in an aspect-corrected box while
    // taps were still matched against the full view, so stars moved under the tap.
    [Theory]
    [InlineData(1900, 300)]
    [InlineData(480, 300)]
    [InlineData(900, 620)]
    [InlineData(1920, 1023)]
    public void Every_star_can_be_tapped_where_it_is_drawn(float width, float height)
    {
        var figure = SkyLayout.Figure(new RectF(0, 0, width, height));

        for (var index = 0; index < LeoConstellation.Stars.Count; index++)
        {
            AssertTapHits(figure, index);
        }
    }

    [Fact]
    public void Bias_moves_the_figure_clear_of_centred_content()
    {
        var rect = new RectF(0, 0, 1920, 1023);
        var centred = SkyLayout.Figure(rect, SkyLayout.DefaultBias);
        var pushed = SkyLayout.Figure(rect, 0.88f);

        Assert.True(pushed.X > centred.X);
        Assert.True(pushed.Right <= rect.Right);
        Assert.Equal(centred.Width, pushed.Width, 2);
    }

    [Fact]
    public void Taps_follow_the_figure_when_it_is_biased()
    {
        var figure = SkyLayout.Figure(new RectF(0, 0, 1920, 1023), 0.88f);

        AssertTapHits(figure, LeoConstellation.RegulusIndex);
        AssertTapHits(figure, 8);
    }

    private static void AssertTapHits(RectF figure, int index)
    {
        var star = LeoConstellation.Stars[index];
        var drawnX = figure.X + star.X * figure.Width;
        var drawnY = figure.Y + star.Y * figure.Height;
        var (x, y) = SkyLayout.Normalize(figure, drawnX, drawnY);
        Assert.Equal(index, LeoConstellation.NearestIndex(x, y, 0.07));
    }

    [Fact]
    public void Market_cap_sets_how_bright_a_holding_burns()
    {
        var rows = PortfolioSky.Rank([Stock("BIG", 3_000_000_000_000), Stock("MID", 90_000_000_000), Stock("SML", 400_000_000)]);

        Assert.Equal(1.0, rows[0].Magnitude, 3);
        Assert.True(rows[1].Magnitude < rows[0].Magnitude);
        Assert.True(rows[2].Magnitude < rows[1].Magnitude);
        Assert.All(rows, row => Assert.InRange(row.StarOpacity, 0.35, 1.0));
    }

    [Fact]
    public void A_lone_holding_is_not_dimmed_by_having_no_rivals()
    {
        var rows = PortfolioSky.Rank([Stock("ONLY", 12_000_000_000)]);

        Assert.Equal(1.0, Assert.Single(rows).Magnitude, 3);
    }

    [Fact]
    public void A_missing_market_cap_still_gets_a_visible_star()
    {
        var rows = PortfolioSky.Rank([Stock("BIG", 3_000_000_000_000), Stock("NONE", 0)]);

        Assert.True(rows[1].Magnitude >= 0.2);
        Assert.True(rows[1].StarSize > 0);
    }

    private static PortfolioStock Stock(string symbol, long marketCap)
    {
        return new PortfolioStock(1, symbol, $"{symbol} Inc.", 10m, 0m, "Test", marketCap);
    }
}
