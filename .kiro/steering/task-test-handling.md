---
inclusion: always
---

# Task Test Handling Rules

- While handling task about test, split testing task from test implementation task, to prevent test failure from blocking test implementation.

- To prevent too many tasks in task list, you can merge testing tasks into the checkpoint tasks.

- Do not try to install any package, ask user to do it.

- Do not use Category attribute for test method unless specific mentioned.

- WHEN running tests via Unity batch mode (`Unity.exe -runTests -batchmode`), THE `-testResults` output path SHALL point into `TestResults/` at the workspace root (e.g. `-testResults "<workspace>/TestResults/<FeatureName>-results.xml"`). Do NOT write results under `Temp/`, because Unity periodically clears `Temp/` and the result XML may disappear before it can be read. Note: Unity batch mode also writes a duplicate copy to the persistentDataPath default location (`%USERPROFILE%/AppData/LocalLow/<Company>/<Product>/TestResults.xml`); ignore that copy and read the `-testResults` path instead. 

- In the final checkpoint task, add a task to generate a requirement test coverage document. This document displays the overall test coverage of requirements and the test methods corresponding to each requirement (e.g. Property N, unit test). This task only prints the document for user reference; it MUST NOT adjust or add any tests as a result.

- The coverage document MUST be placed in the same folder as the spec's `design.md`, with the file name `test-coverage.md`.

## Requirement Test Coverage Document Format

The coverage document generated in the final checkpoint MUST follow this fixed format:

```markdown
# Requirement Test Coverage Report

## Overall Coverage

- Total Requirements: <N>
- Covered Requirements: <M>
- Coverage Rate: <M/N as percentage>

## Coverage by Requirement

| Requirement | Description | Test Methods | Status |
|-------------|-------------|--------------|--------|
| 1.1 | <需求描述> | Property 3, `TestMethodName` | Covered |
| 1.2 | <需求描述> | `AnotherTestMethod` | Covered |
| 2.1 | <需求描述> | - | Not Covered |
```

- The `Test Methods` column lists the corresponding test methods for each requirement, using `Property N` for property-based tests and the test method name (in backticks) for unit tests. Use `-` when no test covers the requirement.
- The `Status` column is `Covered` when at least one test method maps to the requirement, otherwise `Not Covered`.

## Mutation Testing (Final Checkpoint)

In the final checkpoint task, after the requirement test coverage document, add a task to run mutation testing with [Stryker](https://stryker-mutator.io/) (`dotnet stryker`). Once the report is produced, hand the results to the user to decide how to proceed. This task MUST NOT adjust or add any test or functionality code.

### Precondition

- Mutation testing runs only when the functionality under test does **not** depend on `UnityEngine`. Stryker builds a plain .NET project, so `UnityEngine` types are unavailable.
- Before starting the procedure, check whether the functionality code references `UnityEngine` (e.g. `using UnityEngine;` or any `UnityEngine.*` type). WHERE the functionality depends on `UnityEngine`, THE mutation testing task SHALL be skipped, and this MUST be noted in `mutation-test-result.md` (or reported to the user) instead of running Stryker.

### Folder Convention

- Mutation test resources live under `TestProj/`.
- Generated data and results for a feature live in `TestProj/<FeatureName>/` (hereafter the **功能測試資料夾 / feature test folder**).
- `TestProj/TemplateTestProj.csproj` and `TestProj/template-stryker-config.json` are the templates to copy.

### Procedure

1. Copy `TestProj/TemplateTestProj.csproj` and `TestProj/template-stryker-config.json` into the feature test folder `TestProj/<FeatureName>/`.

2. Rename the copied csproj to `TestProj.csproj`. Adjust its `ProjectReference` and `Compile` include paths so they resolve from the feature test folder to the functionality project and the test code folder. Because the feature test folder is one level deeper than `TestProj/`, the relative paths gain an extra `../` (e.g. `../Utils.csproj` becomes `../../Utils.csproj`, and `..\Assets\Scripts\...` becomes `..\..\Assets\Scripts\...`).

3. Rename the copied json to `stryker-config.json`. Adjust its `mutate` globs so they target the functionality being mutated and exclude the test code (e.g. keep a `"!.../Tests/**/*.cs"` exclusion).

4. Run mutation testing with `dotnet stryker` from the feature test folder.

5. Analyze the Stryker result report.

   5-1. Export a fixed-format report and save it as `mutation-test-result.md` in the same folder as the spec's `design.md`.

   5-2. The report MUST include the overall mutation score, and list the risky items among the Undetected mutations, giving each item's location, potential risk, and suggested handling.

- Do not install any package for mutation testing; if a tool such as the Stryker CLI is missing, ask the user to install it.

### Mutation Test Result Document Format

The `mutation-test-result.md` generated in the final checkpoint MUST follow this fixed format:

```markdown
# Mutation Test Result Report

## Overall Score

- Mutation Score: <percentage>
- Killed: <N>
- Survived: <N>
- Timeout: <N>
- No Coverage: <N>

## Risky Undetected Mutations

| Location | Mutation | Potential Risk | Suggested Handling |
|----------|----------|----------------|--------------------|
| `File.cs:12` | <變異描述> | <可能風險> | <建議處理方式> |
```

- The `Risky Undetected Mutations` table lists only the Undetected (Survived / No Coverage) mutations that carry real risk. When there are none, state `無具風險的 Undetected mutation`.
