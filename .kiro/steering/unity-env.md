---
inclusion: manual
---

# 專案Unity環境
此文件包含專案的Unity環境說明，進行設計與調整時請遵循現有環境進行。

####### 給開發者看的comment，AI請跳過 #######
- 此文件提供Unity環境steering範本，根據專案設定後，把本行移除，再開成always include

####### comment結尾 #######
- Unity Version: Unity 6000.5.71f1
- Input: New Input System
- UI: UI toolkit
- Test: NUnit with FsCheck 3.X, tested by Unity Test Runner
    - Do not use FsCheck.NUnit, just call FsCheck API in NUnit test if PBT is required
    - namespace for FsCheck 3.X in C#: FsCheck.Fluent
    - The test classes will be in different assembly from functionality class, to utilize Unity Test Runner
- Other Packages: UniTask, Addresable, DOTween, Odin Inspector
