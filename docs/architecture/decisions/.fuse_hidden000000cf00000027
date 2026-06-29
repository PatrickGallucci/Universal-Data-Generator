# ADR-0003: Boost overlap is strongest-wins

## Status

Accepted

## Context

Two boost dates can put their windows on the same day. The factors must combine. Compounding runs
away quickly: two 50 percent boosts would reach 2.25 times baseline.

## Decision

When multiple boosts cover a day, take the single boost with the largest absolute deviation from
baseline that day. Do not sum or compound. The strongest event wins, and a suppression can override a
boost.

## Consequences

- Two coincident 50 percent boosts stay at 1.5 times, not 2.25.
- A negative boost (suppression) overrides a positive boost of smaller magnitude on the same day.
- The behavior is unit-tested.

## Alternatives considered

- Sum the percentages. Predictable but lets overlapping events inflate volume without intent.
- Compound the factors. Multiplies quickly and is hard to reason about.
