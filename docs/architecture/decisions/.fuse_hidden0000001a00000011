# ADR-0008: Embedded JSON catalog, no spreadsheet

## Status

Accepted

## Context

The catalog (industries, source types, storage targets, entities, and the attribute schema) originally lived in an
Excel workbook that the engine read at runtime with an OpenXML reader. That coupled every host to a file path and
to the `DocumentFormat.OpenXml` dependency, and a corrupted workbook could break a run. The reference data is
static between releases.

## Decision

Ship the catalog as JSON embedded in `UniDataGen.Configuration` and build it through `CatalogStore.Load()`. The data
is split into `industries.json`, `source-types.json`, `storage.json`, `entities.json`, and a gzip-compressed
`attributes.json.gz` for the large attribute schema. Remove `WorkbookCatalogReader` and the OpenXML dependency.
Hosts no longer take a workbook path.

## Consequences

- The generator has no dependency on a spreadsheet, and no host needs a workbook path or `DATAGEN_WORKBOOK` setting.
- The catalog is edited by changing the JSON resources and rebuilding; it is the single source of truth.
- The attribute schema ships gzip-compressed (about 3 MB) and is decompressed once on first access.
- The catalog editor's industry and entity dropdowns read the same JSON through `ReferenceCatalog`.

## Alternatives considered

- Keep reading the workbook at runtime. Couples hosts to a file and a parsing dependency, and is fragile if the
  workbook is moved or corrupted.
- Load the JSON from disk at a configurable path rather than embedding. More flexible but reintroduces a file
  dependency for static data; embedding keeps the engine self-contained.
