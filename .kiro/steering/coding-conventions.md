---
inclusion: always
---

# Coding Conventions

## Environment
- 除非特別指定，否則預設環境如下
- 語言：C#
- 平台：Unity（使用 Unity 相容的 C# 版本，避免使用 Unity 不支援的 .NET API）
- 每個 MonoBehaviour、ScriptableObject 等 Unity 類別遵循 Unity 生命週期慣例
- 程式根目錄：如果workspace下存在Assets資料夾，則程式根目錄為Asstes/Scripts，否則程式根目錄為Scripts/
- 將所有生成的程式放在程式根目錄下，預設依照規格名稱拆分子資料夾，如有特別指定子資料夾，則以指定規則為主

## Namespace and Assembly Splitting

- 根據功能與 `UnityEngine` 的相依情況拆分 namespace 與 asmdef：
  - 將功能中**不相依 `UnityEngine`** 的部分（純邏輯、資料模型、演算法等）獨立出來，給予專屬的 namespace 與 asmdef，使其可在純 .NET 環境（如單元測試、PBT、mutation testing）下編譯與測試。
  - 將**相依 `UnityEngine`** 的部分（MonoBehaviour、ScriptableObject、Unity API 呼叫等）放在另一組 namespace 與 asmdef。
- 例外：WHERE 整個功能高度相依 `UnityEngine`（拆分後獨立部分幾乎為空或無實質邏輯），THE 功能 SHALL 不額外拆分，維持單一 namespace 與 asmdef。
- 拆分後的獨立部分不得 `using UnityEngine` 或引用任何 `UnityEngine.*` 型別。
- asmdef 命名與 namespace 命名應對應，並清楚反映其相依性（例如獨立部分與 Unity 相依部分以後綴或子命名空間區分）。

## Coding Style

本專案的 Coding Style 依循 StyleCop.Analyzers（JoJorge 版本）規範，分為以下幾個 rule area：

#[[file:DesignData/CodingStyle/SpacingRules.md]]

#[[file:DesignData/CodingStyle/ReadabilityRules.md]]

#[[file:DesignData/CodingStyle/OrderingRules.md]]

#[[file:DesignData/CodingStyle/NamingRules.md]]

#[[file:DesignData/CodingStyle/MaintainabilityRules.md]]

#[[file:DesignData/CodingStyle/LayoutRules.md]]

#[[file:DesignData/CodingStyle/DocumentationRules.md]]

## Magic Numbers

- 避免在程式碼中使用 magic number（未命名的數字常量）
- 將數值常量提取為具有語義名稱的 `const` 或 `static readonly` 欄位
- 例外：0、1、-1 等在語境中意義明確的值（如索引起始、計數初始化）可不提取
- 字串常量同理，重複使用的字串應提取為 `const` 欄位
