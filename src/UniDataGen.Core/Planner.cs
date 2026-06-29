using UniDataGen.Abstractions;

namespace UniDataGen.Core;

/// <summary>The records owed for one step or fire, per action.</summary>
public readonly record struct OwedCounts(long New, long Update, long Delete)
{
    /// <summary>True when nothing is owed for any action.</summary>
    public bool IsEmpty => New == 0 && Update == 0 && Delete == 0;

    public long Total => New + Update + Delete;
}

/// <summary>Pure helpers that turn rates and an effective multiplier into owed counts. No I/O, fully testable.</summary>
public static class Planner
{
    /// <summary>Owed counts for a fired batch given its per-batch rates and the effective multiplier at the fire time.</summary>
    public static OwedCounts BatchOwed(BatchSettings batch, double effective) => new(
        BatchPlanner.BatchSize(batch.PerBatch.New, effective),
        BatchPlanner.BatchSize(batch.PerBatch.Update, effective),
        BatchPlanner.BatchSize(batch.PerBatch.Delete, effective));
}
