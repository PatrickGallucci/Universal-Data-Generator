# API: UniDataGen.Core

The engine. The pure helpers have no I/O and are unit-tested; the orchestrator owns the run loop and
logging.

## Pure helpers

| Type | Key members | Purpose |
|------|-------------|---------|
| `DayPartCalculator` | `FromHour(int)`, `FromTime(DateTimeOffset)` | Maps an hour to a day part. |
| `ScheduleEvaluator` | `ProfileFor(...)`, `Weight(...)` | Resolves the day-part multiplier. |
| `BoostCalculator` | `PercentOn(BoostDate, DateOnly)`, `FactorOn(...)` | The boost ramp and overlap rule. |
| `EffectiveMultiplier` | `At(EntityProfile, DateTimeOffset)` | Day-part weight times boost factor. |
| `RateAccumulator` | `Advance(rate, effective, elapsed)` | Real-time fractional accrual. |
| `BatchPlanner` | `Interval(...)`, `DueFires(...)`, `BatchSize(...)` | Batch cadence and size. |
| `Planner` | `BatchOwed(BatchSettings, double)` | Owed counts for a fired batch. |
| `AttributeClassifier` | `IsSystemField(...)`, `SemanticAttributes(...)` | Routes attributes to engine or provider. |
| `SystemFieldStamper` | `Stamp(...)` | Stamps keys, audit, and state. |
| `TimeZoneResolver` | `ToLocal(...)` | Converts an instant to local wall-clock. |

## Stateful types

```csharp
public sealed class IdentityPool
{
    public IdentityPool(int capacity = 100_000, int? seed = null);
    public int Count { get; }
    public void Add(object key);
    public bool TryPeek(out object key);
    public bool TryTake(out object key);
}

public sealed class ValueCoordinator
{
    public ValueCoordinator(IValueProvider provider, ILogger? logger = null);
    public Task<EntityBatch> BuildAsync(EntityInfo entity, string industry, string sourceType,
        long newCount, long updateCount, long deleteCount, IdentityPool pool, DateTimeOffset at,
        CancellationToken cancellationToken);
}

public sealed class GenerationOrchestrator
{
    public GenerationOrchestrator(Catalog catalog, IValueProvider provider, ISinkFactory sinkFactory,
        ILoggerFactory? loggerFactory = null);
    public Task RunAsync(RunConfiguration config, TimeSpan tickInterval, CancellationToken cancellationToken);
}
```

`RunAsync` validates the run header, opens every enabled sink, and runs each entity concurrently until
cancellation. It attaches a run-scoped correlation id to every log entry.
