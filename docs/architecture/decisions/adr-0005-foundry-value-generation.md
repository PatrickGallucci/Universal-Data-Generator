# ADR-0005: Foundry produces semantic values

## Status

Accepted

## Context

Each entity has many attributes with CDM data types and a purpose. Some are identity and audit fields
that must line up with the generation clock and the key space; others are semantic fields such as
names, descriptions, and amounts that should look plausible for the industry.

## Decision

Split each record. The engine stamps system fields: primary keys, audit timestamps by purpose, state
codes, and foreign keys drawn from the identity pool. Azure AI Foundry produces the semantic fields
through a structured-output call whose JSON schema is derived from the entity's semantic attributes.
The engine validates and coerces every returned value to its CDM type.

## Consequences

- Keys and timestamps are correct and cheap, because the engine computes them rather than asking a model.
- The model focuses on the values it is good at, which lowers token cost and improves realism.
- Generation is repeatable in structure and distribution, not bit for bit, because model output is not
  deterministic.

## Alternatives considered

- Ask the model for every field, including keys and timestamps. Slower, costlier, and less correct.
- Rule-based generation for all fields. Fast and offline but less realistic for free-text fields. The
  offline providers in the hosts use this approach for dry runs.
