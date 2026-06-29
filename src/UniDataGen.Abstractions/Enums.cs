namespace UniDataGen.Abstractions;

/// <summary>How an entity emits data over time.</summary>
public enum GenerationMode
{
    /// <summary>Continuous accrual of records against a per-hour rate.</summary>
    RealTime,

    /// <summary>Discrete batches fired on a cadence.</summary>
    Batch
}

/// <summary>Whether the schedule uses one weekly profile or separate weekday and weekend profiles.</summary>
public enum ScheduleSplit
{
    WholeWeek,
    WeekdayWeekend
}

/// <summary>The four six-hour parts of a day. Night 00:00-06:00, Morning 06:00-12:00, Afternoon 12:00-18:00, Evening 18:00-24:00.</summary>
public enum DayPart
{
    Night,
    Morning,
    Afternoon,
    Evening
}

/// <summary>The record operation generated for a target.</summary>
public enum RecordAction
{
    New,
    Update,
    Delete
}

/// <summary>The time unit a batch cadence counts against.</summary>
public enum BatchUnit
{
    Hour,
    Day,
    Week,
    Month
}

/// <summary>Whether a target is naturally streaming or naturally batch oriented.</summary>
public enum SinkKind
{
    Streaming,
    Batch
}
