# E2E Regression Contract

`function_mapping.json` 的 key 是唯一 E2E target 定義。

## 1. Target topology

- `workflow_blocks = 1`：獨立 block，單獨測。
- `workflow_blocks > 1`：非獨立 composite，必須依 mapping 順序一起測。
- 禁止 AI 為了增加 coverage 自行串接其他 block。
- 禁止把 composite 拆成獨立 E2E。

單積木 Workflow/SOP 只分 `Action` 或 `Condition`。可共用的建立 SQL 放 `Global/Workflow/` 並直接 reuse。
只有 composite topology 無法由 Global template 表達時，才建立 target-specific Workflow SQL。

## 2. Case = prepare.sql + mock

每個 Case：

```text
<TARGET>/
  <TARGET>.vb
  <TARGET>-SOP-001/
    prepare.sql
    mock_<external>.*   # optional
```

- `prepare.sql`：block parameters、case-specific DB before-state；只允許 INSERT。
- `mock_*`：API/HTTP、MQ、Kafka、NATS、WSDL/SOAP、gRPC、第三方 boundary。
- 每個 SOP 完全獨立，不可共用 runtime state。

不可 mock：DB、Repository、Business、Block、Workflow、Main、mapped Entry/Critical function。

## 3. Test execution

```text
[optional external mock]
-> own prepare.sql
-> runtime/env parameters
-> CallMain("<TARGET>")
-> Assert observable production post-state
```

不得直接呼叫 Block.Execute / Business / Repository 來製造 coverage。

## 4. Composite

Composite 共用同一個 runtime DB、WorkflowContext、Main call，並完全照 `workflow_blocks` 順序執行。

例如：

```text
TokenGenerate_CommandSubmit
= TokenGenerate -> CommandSubmit
```

## 5. Coverage

同一次 validation 驗證 target 的全部 `entry_functions + critical_functions`；每個 mapped function 必須嚴格 `> min_coverage`。

## 6. Structure Gate

Python 只檢查 filename/folder：
- `<TARGET>/<TARGET>.vb`
- 至少一個 `<TARGET>-SOP-NNN/prepare.sql`
- SOP 只允許 `prepare.sql` 與 `mock_*`

Python 不解析 TestMethod、DisplayName、CallMain 或 Workflow 語意；交給 AI Grill/Review。

## 7. Protected inputs

AI 只能建立 TestProject target artifacts；不得修改 material、mapping、validator、tools、TestInfrastructure、Test/TestProject/*.vbproj、PROJECT_CONTRACT.md。
