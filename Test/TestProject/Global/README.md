# Global shared Workflow/SOP templates

`Global/Workflow/` 保留給真正可跨 E2E target 共用的 Workflow/SOP 建立 SQL。

主要模型只有：

```text
Action
Condition
```

原則：
- single block 優先 reuse Action/Condition Global SQL；
- 不要為每個 SOP Case 複製相同 Workflow SQL；
- Case-specific block parameters / DB before-state 放 `prepare.sql`；
- external response 放 `mock_*`；
- 少數非獨立 composite 若 Global 無法表示，才使用 target-specific Workflow SQL；
- 禁止為了測試而隨機串接本來獨立的 blocks。

Global SQL 由專案提供/維護；AI 不應擅自改寫共用 schema/template。
