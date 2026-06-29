using UniDataGen.Abstractions;

namespace UniDataGen.Core;

/// <summary>Resolves the day-part multiplier for an entity schedule at a given local time.</summary>
public static class ScheduleEvaluator
{
    /// <summary>Picks the weight set in force for a day of week, honoring the whole-week or weekday/weekend split.</summary>
    public static DayPartWeights ProfileFor(EntitySchedule schedule, DayOfWeek dayOfWeek)
    {
        if (schedule.Split == ScheduleSplit.WholeWeek)
        {
            return schedule.Week ?? DayPartWeights.Flat;
        }

        bool isWeekend = dayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        return (isWeekend ? schedule.Weekend : schedule.Weekday) ?? DayPartWeights.Flat;
    }

    /// <summary>Returns the day-part multiplier for a local time.</summary>
    public static double Weight(EntitySchedule schedule, DateTimeOffset localTime)
    {
        DayPartWeights profile = ProfileFor(schedule, localTime.DayOfWeek);
        DayPart part = DayPartCalculator.FromTime(localTime);
        return profile.For(part);
    }
}
