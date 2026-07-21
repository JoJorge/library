---
inclusion: always
---

# Language Conventions

## General Rule

Write all content in **Traditional Chinese (繁體中文)** by default. This applies to:
- Body text, descriptions, and explanations
- Table cell content
- Code comments
- Glossary definitions
- User stories and acceptance criteria descriptions

## Exceptions — Use English For

The following elements MUST remain in English regardless of context:

**Section headings** — All `#`, `##`, `###`, `####` headings in spec documents must be in English.
Examples: `## Overview`, `## Architecture`, `## Components and Interfaces`, `## Data Models`, `## Correctness Properties`, `## Error Handling`, `## Testing Strategy`, `## Introduction`, `## Glossary`, `## Requirements`

**EARS keywords** — The following requirement syntax keywords must always be in English:
`WHEN`, `WHERE`, `WHILE`, `IF`, `THEN`, `THE`, `SHALL`

**Technical terms and identifiers** — Class names, method names, variable names, file names, library names, framework names, and any code identifiers stay in English.

**Acronyms and proper nouns** — e.g., DCL, JVM, PBT, Java, JUnit, jqwik, Singleton, volatile, synchronized

## Acceptance Criteria Format

Write acceptance criteria in mixed format: EARS keywords in English, everything else in Chinese.

Example:
> WHEN 單例第一次被存取時，THE 延遲初始化單例 SHALL 建立實例

## Spec Document Headings Reference

Use the following English headings for spec documents:

**requirements.md:**
- `## Introduction`
- `## Glossary`
- `## Requirements`
- `### Requirement N: [Title in English]`
- `#### Acceptance Criteria`

**design.md:**
- `## Table of Contents` (or `## 目錄` — exception allowed for TOC)
- `## Overview`
- `## Architecture`
- `## Components and Interfaces`
- `## Data Models`
- `## Correctness Properties`
- `### Property N: Title in English` — individual properties inside Correctness Properties
- `## Error Handling`
- `## Testing Strategy`
