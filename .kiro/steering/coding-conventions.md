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

## Coding Style

本專案的 Coding Style 依循 StyleCop.Analyzers（JoJorge 版本）規範，分為以下幾個 rule area：

#[[file:DesignData/CodingStyle/SpecialRules.md]]

#[[file:DesignData/CodingStyle/SpacingRules.md]]

#[[file:DesignData/CodingStyle/ReadabilityRules.md]]

#[[file:DesignData/CodingStyle/OrderingRules.md]]

#[[file:DesignData/CodingStyle/NamingRules.md]]

#[[file:DesignData/CodingStyle/MaintainabilityRules.md]]

#[[file:DesignData/CodingStyle/LayoutRules.md]]

#[[file:DesignData/CodingStyle/DocumentationRules.md]]
