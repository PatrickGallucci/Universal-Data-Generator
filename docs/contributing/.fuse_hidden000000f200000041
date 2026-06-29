# Style guide

This guide covers the code and documentation conventions.

## C# code

- Use the latest C# features. Prefer file-scoped namespaces and single-line using directives.
- Use `is null` and `is not null`, not `== null`.
- Use pattern matching and switch expressions where they read well.
- Use `nameof` instead of string literals for member names.
- Put a newline before the opening brace of a block.
- Put the final return statement of a method on its own line.
- Write XML doc comments on public APIs, with `<example>` and `<code>` where they help.
- Use `ConfigureAwait(false)` in library code.
- Handle edge cases and exceptions. Do not swallow exceptions silently.

Format with the rules in `.editorconfig`.

## Naming

- PascalCase for types, methods, and public members.
- camelCase for private fields and locals.
- Prefix interfaces with `I`.

## Documentation

Follow the documentation-standards skill:

- Active voice: "The system creates a log entry", not "A log entry is created".
- Present tense: "The function returns", not "The function will return".
- Second person: "You can configure", not "Users can configure".
- Be specific. Avoid "might", "probably", and "sometimes".
- One H1 per document. H2 for sections, H3 for subsections, H4 sparingly.
- Specify the language on every fenced code block.
- Use relative links for in-repo files and full URLs for external resources.
- Create diagrams with Mermaid only. Do not use icons.
- Keep the changelog in Keep a Changelog form with Semantic Versioning.

## Before you publish documentation

- Run a spell check.
- Verify internal and external links.
- Confirm code examples compile or run.
- Remove any sensitive information.
- Update the table of contents and version numbers.
