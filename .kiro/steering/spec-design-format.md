---
inclusion: always
---

# Spec Design Document Format Rules

When generating or updating any `design.md` spec document, apply the following formatting rules:

## Table of Contents

Every `design.md` MUST include a `## 目錄` (Table of Contents) section immediately after the document title (`# ...`) and before the `## 概覽` section.

The TOC must list all `##` and `###` headings as anchor links, with `###` entries indented by two spaces. Separate the TOC from `## 概覽` with a `---` horizontal rule.

Example structure:
```markdown
# 技術設計文件：...

## 目錄

- [概覽](#概覽)
- [架構](#架構)
  - [整體結構](#整體結構)
- ...

---

## 概覽
...
```

## Back-to-Top Links

Every `##` section (except `## 目錄` itself) MUST end with a back-to-top link immediately before the `---` horizontal rule that separates it from the next section. The link format is:

```markdown
[↩](#目錄)
---
```

The `[↩](#目錄)` line comes first, then `---` on the very next line, with no blank line between them. Place a blank line before `[↩](#目錄)`.

Example:
```markdown
## 概覽

*內容說明。*

[↩](#目錄)
---

## Architecture
...
```

The last `##` section in the document also ends with `[↩](#目錄)` followed by `---`.

## Heading Hierarchy for Correctness Properties

Inside the `## Correctness Properties` section, each individual property MUST use `###` (h3) headings with the English format `Property N: <English title>`. The title may include a Chinese subtitle after an em dash.

The validation line immediately following the property description MUST use bold English format: `**Validates: Requirements X.X, Y.Y**`.

Correct:
```markdown
## Correctness Properties

### Property 1: Title in English

*中文描述。*

**Validates: Requirements 1.2, 2.3**

---

### Property 2: Title in English — 中文副標

*中文描述。*

**Validates: Requirements 1.4, 4.3**
```

Incorrect:
```markdown
## Correctness Properties

#### 屬性 1：中文標題

**驗證：需求 1.2、2.3**
```

TOC entries for properties use indented links under `## Correctness Properties`:
```markdown
- [Correctness Properties](#correctness-properties)
  - [Property 1: Title](#property-1-title)
  - [Property 2: Title](#property-2-title)
```

## UML Diagrams

### File Format and Location

UML diagrams are written in Mermaid syntax. Placement depends on total line count:

| Total UML lines | Placement |
|-----------------|-----------|
| < 300 | Inline in `design.md` as a fenced `mermaid` code block |
| 300 – 999 | Single `.mmd` file in `DesignData/`, referenced from `design.md` |
| ≥ 1000 | Multiple `.mmd` files in `DesignData/` with a numeric suffix |

When splitting into multiple files, use a numeric suffix starting from `_2`:

```
DesignData/
  FeatureName.mmd        ← part 1 (≤ 1000 lines)
  FeatureName_2.mmd      ← part 2
  FeatureName_3.mmd      ← part 3
```

Each split file must be a self-contained, valid Mermaid diagram (i.e., starts with `classDiagram`).

When a diagram is extracted to `DesignData/`, reference it from `design.md` with a relative path link:

```markdown
![UML Diagram](DesignData/FeatureName.mmd)
```
UML or its link should be presented in Architecture/整體結構 section


### Diagram Structure

Every `classDiagram` MUST follow this block ordering:

1. **Namespace declarations** — group classes into logical namespaces
2. **Enum definitions** — `<<enum>>`
3. **Interface definitions** — `<<interface>>`
4. **Class definitions** — `<<abstract>>`, concrete, generic
5. **Relations** — all relationship lines
6. **Style class assignments** — `class Foo:::StyleName`
7. **Style definitions** — `classDef` declarations

Separate each block with a labelled comment banner:

```mermaid
%% =========================================================
%% Block Name
%% =========================================================
```

### Member Syntax

Use the **attribute-first** style for fields and the **return-type-last** style for methods, consistent with the UML example:

```
- fieldName: Type          ← field
+ MethodName(Type param) ReturnType   ← method
+ AbstractMethod() ReturnType*        ← abstract method (trailing *)
+ StaticField: Type$                  ← static member (trailing $)
```

Visibility prefixes: `+` public, `-` private, `#` protected, `~` internal.

### Modification Markers

When a diagram documents changes to an existing architecture, apply the following conventions.

**Class-level style** — assign one of three `classDef` styles to mark the state of each changed class:

| State | Style name | Appearance |
|-------|-----------|------------|
| 新增 (Added) | `NewClass` | Dark green fill |
| 修改 (Modified) | `ModifiedClass` | Teal fill |
| 移除 (Removed) | `RemovedClass` | Muted red fill, dashed border |

Unchanged classes and all-new architectures use the default style (no assignment needed).

Always declare `classDef` at the bottom of the file:

```
classDef NewClass fill:#407E40,stroke:#006400,stroke-width:2px
classDef ModifiedClass fill:#008B8B,stroke:#004B4B,stroke-width:2px
classDef RemovedClass fill:#AF6676,stroke:#DC143C,stroke-width:2px,stroke-dasharray: 5 5
```

**Member-level markers** — inside a `ModifiedClass`, append an emoji suffix to each changed member:

| Change | Suffix |
|--------|--------|
| 新增 (Added) | `❇️` |
| 修改 (Modified) | `🔄` |
| 移除 (Removed) | `❌` |

Unchanged members carry no suffix. Members in `NewClass` or `RemovedClass` never carry per-member suffixes.

Example:

```
class ModifiedClass1 {
    <<修改>>
    - existingField: int
    - addedField: string ❇️
    + ChangedMethod(NewParam p) ReturnType 🔄
    + RemovedMethod() void ❌
}
class ModifiedClass1:::ModifiedClass
```

### Relation Symbols Reference

| Relation | Syntax | Notes |
|----------|--------|-------|
| 繼承 Inheritance | `Parent <\|-- Child` | Solid line, hollow triangle |
| 實作 Realization | `IFoo <\|.. ConcreteClass` | Dashed line, hollow triangle |
| 組合 Composition | `Whole *-- Part` | Solid line, filled diamond |
| 聚合 Aggregation | `Container o-- Element` | Solid line, hollow diamond |
| 關聯 Association | `A --> B` | Solid arrow (unidirectional) |
| 雙向關聯 Bidirectional | `A -- B` | Solid line, no arrow |
| 依賴 Dependency | `A ..> B` | Dashed arrow |

Multiplicity labels go on both ends when relevant:

```
ClassA "1" --> "0..*" ClassB : label
```
