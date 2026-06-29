# ADR-0004: Day-part and boost scale batch size

## Status

Accepted

## Context

Batch mode fires on a cadence of N per hour, day, week, or month, and emits records per batch per
action. The day-part weight and the boost factor have to apply somewhere: to the size of each batch,
or to how often batches fire.

## Decision

The day-part weight and boost factor scale the size of each batch at its fire time. The cadence stays
fixed at the configured frequency.

## Consequences

- A nightly load stays recognizably nightly; the boost makes the batch fatter or thinner.
- The cadence is simple to reason about and does not drift with the schedule.
- Real-time mode is unaffected; it accrues against the per-hour rate.

## Alternatives considered

- Scale the cadence and keep batch size fixed. Rejected because a varying cadence is harder to reason
  about and blurs the meaning of "four per day".
