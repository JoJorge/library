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
- UI: UGUI
- Test: NUnit with FsCheck,(do not use FsCheck.NUnit, just call FsCheck Check API in NUnit test if PBT is required)
- Other Packages: UniTask, Addresable, DOTween
