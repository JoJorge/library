# Special Rules (SA0000-)

提供特殊功能的規則，包含組態錯誤回報、功能開關等。

---

## SA0001 — XmlCommentAnalysisDisabled

因目前專案組態，所有 XML 文件註解的診斷已被停用。當專案未設定輸出 XML 文件檔時（`DocumentationMode` 為 `None`），本規則觸發。長期下來文件註解容易累積錯誤。

**範例（專案設定）：**
```xml
<PropertyGroup>
  <!-- 產生 XML 文件並壓制缺少文件的警告 CS1573/CS1591/CS1712 -->
  <DocumentationFile>$(OutputPath)$(AssemblyName).xml</DocumentationFile>
  <NoWarn>$(NoWarn),1573,1591,1712</NoWarn>
</PropertyGroup>
```

---

## SA0002 — InvalidSettingsFile

`stylecop.json` 設定檔因反序列化錯誤無法載入。發生時各 analyzer 會自動回退為預設設定，可能不符使用者預期。

**範例（合法的最小 stylecop.json）：**
```json
{
  "$schema": "https://raw.githubusercontent.com/DotNetAnalyzers/StyleCopAnalyzers/master/StyleCop.Analyzers/StyleCop.Analyzers/Settings/stylecop.schema.json",
  "settings": {
    "documentationRules": {
      "companyName": "MyCompany"
    }
  }
}
```

---

*來源：[`documentation/SpecialRules.md`](../documentation/SpecialRules.md)、[`documentation/SA0001.md`](../documentation/SA0001.md)、[`documentation/SA0002.md`](../documentation/SA0002.md)*
