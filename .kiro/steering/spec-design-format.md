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

## Heading Hierarchy for Correctness Properties

Inside the `## 正確性屬性` section, each individual property MUST use `####` (h4) headings, not `###` (h3).

Correct:
```markdown
## 正確性屬性

#### 屬性 1：...

#### 屬性 2：...
```

Incorrect:
```markdown
## 正確性屬性

### 屬性 1：...
```

This ensures properties are visually subordinate to the section heading and the TOC hierarchy is consistent.
