using UniDataGen.Abstractions;
using UniDataGen.Core;
using Xunit;

namespace UniDataGen.Core.Tests;

public class BoostCalculatorTests
{
    private static DateOnly D(string s) => DateOnly.Parse(s);

    // Worked example one: positive boost, 25 percent shoulder, 50 percent peak, 14 before, 6 after.
    private static readonly BoostDate Positive = new(D("2026-08-15"), 25, 14, 6, 50);

    // Worked example two: suppression, -25 percent shoulder, -100 percent peak, 3 before, 3 after.
    private static readonly BoostDate Negative = new(D("2026-08-15"), -25, 3, 3, -100);

    [Theory]
    [InlineData("2026-08-01", 1.0)]   // first before day
    [InlineData("2026-08-08", 13.92)] // midpoint of the up-ramp
    [InlineData("2026-08-14", 25.0)]  // last before day
    [InlineData("2026-08-15", 50.0)]  // peak
    [InlineData("2026-08-16", 25.0)]  // first after day
    [InlineData("2026-08-21", 1.0)]   // last after day (symmetric: 6 days)
    public void Positive_window_percentages(string day, double expected)
        => Assert.Equal(expected, BoostCalculator.PercentOn(Positive, D(day)), 2);

    [Theory]
    [InlineData("2026-07-31")] // day before window starts
    [InlineData("2026-08-22")] // symmetric end excludes this day
    public void Positive_outside_window_is_zero(string day)
    {
        Assert.Equal(0.0, BoostCalculator.PercentOn(Positive, D(day)), 6);
        Assert.Equal(1.0, BoostCalculator.FactorOn(new[] { Positive }, D(day)), 6);
    }

    [Theory]
    [InlineData("2026-08-12", -1.0)]
    [InlineData("2026-08-13", -13.0)]
    [InlineData("2026-08-14", -25.0)]
    [InlineData("2026-08-15", -100.0)]
    [InlineData("2026-08-16", -25.0)]
    [InlineData("2026-08-18", -1.0)]
    public void Negative_window_percentages(string day, double expected)
        => Assert.Equal(expected, BoostCalculator.PercentOn(Negative, D(day)), 2);

    [Fact]
    public void Negative_peak_factor_is_zero()
        => Assert.Equal(0.0, BoostCalculator.FactorOn(new[] { Negative }, D("2026-08-15")), 6);

    [Fact]
    public void Positive_peak_factor_is_one_point_five()
        => Assert.Equal(1.5, BoostCalculator.FactorOn(new[] { Positive }, D("2026-08-15")), 6);

    [Fact]
    public void Overlap_strongest_wins_by_absolute_deviation()
    {
        var small = new BoostDate(D("2026-08-15"), 20, 0, 0, 20);
        var large = new BoostDate(D("2026-08-15"), 40, 0, 0, 40);
        Assert.Equal(1.4, BoostCalculator.FactorOn(new[] { small, large }, D("2026-08-15")), 6);
    }

    [Fact]
    public void Overlap_suppression_can_override_a_boost()
    {
        var boost = new BoostDate(D("2026-08-15"), 20, 0, 0, 20);
        var suppress = new BoostDate(D("2026-08-15"), -50, 0, 0, -50);
        Assert.Equal(0.5, BoostCalculator.FactorOn(new[] { boost, suppress }, D("2026-08-15")), 6);
    }
}
