# ADR-0002: Symmetric boost window

## Status

Accepted

## Context

A boost date ramps volume up before an event, peaks on the day, and ramps down after. The window is
described by `daysBefore` and `daysAfter`. The original worked examples ended the ramp-down one day
later than a symmetric reading of the counts predicted.

## Decision

The window is symmetric. `daysAfter` is the count of ramp-down days after the peak, with the peak day
excluded from both shoulders. For `daysBefore 14`, the ramp-up covers the 14 days ending the day
before the peak; for `daysAfter 6`, the ramp-down covers the 6 days starting the day after the peak.

## Consequences

- The math is predictable and matches `daysBefore`.
- The one-day-longer end dates in the original examples are not the behavior.
- The `BoostCalculator.PercentOn` function is unit-tested against both worked examples and the
  suppression case.

## Alternatives considered

- An inclusive end that reproduces the original example end dates. Rejected as an off-by-one rather
  than an intended asymmetry.
