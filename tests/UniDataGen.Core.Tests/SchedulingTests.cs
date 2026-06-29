using UniDataGen.Abstractions;
using UniDataGen.Core;
using Xunit;

namespace UniDataGen.Core.Tests;

public class DayPartTests
{
    [Theory]
    [InlineData(0, DayPart.Night)]
    [InlineData(5, DayPart.Night)]
    [InlineData(6, DayPart.Morning)]
    [InlineData(11, DayPart.Morning)]
    [InlineData(12, DayPart.Afternoon)]
    [InlineData(17, DayPart.Afternoon)]
    [InlineData(18, DayPart.Evening)]
    [InlineData(23, DayPart.Evening)]
    public void Hour_maps_to_part(int hour, DayPart expected)
        => Assert.Equal(expected, DayPartCalculator.FromHour(hour));

    [Theory]
    [InlineData(-1)]
    [InlineData(24)]
    public void Invalid_hour_throws(int hour)
        => Assert.Throws<ArgumentOutOfRangeException>(() => DayPartCalculator.FromHour(hour));
}

public class ScheduleEvaluatorTests
{
    [Fact]
    public void WholeWeek_uses_single_profile()
    {
        var schedule = new EntitySchedule(ScheduleSplit.WholeWeek,
            Week: new DayPartWeights(1.0, 2.0, 0.5, 0.1));
        // 2026-08-12 is a Wednesday, 14:00 is Afternoon.
        var t = new DateTimeOffset(2026, 8, 12, 14, 0, 0, TimeSpan.Zero);
        Assert.Equal(2.0, ScheduleEvaluator.Weight(schedule, t), 6);
    }

    [Fact]
    public void WeekdayWeekend_picks_weekend_on_saturday()
    {
        var schedule = new EntitySchedule(ScheduleSplit.WeekdayWeekend,
            Weekday: new DayPartWeights(1.0, 1.0, 1.0, 1.0),
            Weekend: new DayPartWeights(0.6, 0.9, 1.2, 0.2));
        // 2026-08-15 is a Saturday, 20:00 is Evening.
        var t = new DateTimeOffset(2026, 8, 15, 20, 0, 0, TimeSpan.Zero);
        Assert.Equal(1.2, ScheduleEvaluator.Weight(schedule, t), 6);
    }

    [Fact]
    public void WeekdayWeekend_picks_weekday_on_tuesday()
    {
        var schedule = new EntitySchedule(ScheduleSplit.WeekdayWeekend,
            Weekday: new DayPartWeights(1.0, 1.4, 0.8, 0.1),
            Weekend: new DayPartWeights(0.6, 0.9, 1.2, 0.2));
        // 2026-08-11 is a Tuesday, 09:00 is Morning.
        var t = new DateTimeOffset(2026, 8, 11, 9, 0, 0, TimeSpan.Zero);
        Assert.Equal(1.0, ScheduleEvaluator.Weight(schedule, t), 6);
    }
}
