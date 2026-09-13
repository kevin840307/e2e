請產生 TokenGenerate_CommandSubmit E2E Regression Unit Test。

## Target

- E2E target：`TokenGenerate_CommandSubmit`
- Production workflow blocks：`TokenGenerate -> CommandSubmit`
- External mock boundaries in current evidence：`mq`

這是 Composite E2E。Main 必須建立同一個 WorkflowContext，並依序執行：TokenGenerate -> CommandSubmit。禁止為這些積木建立獨立 E2E target，也禁止只測其中一個。

## Working Paths

路徑以本次 task context 為準，不可假設固定磁碟、使用者帳號或父目錄層數。不要建立額外 `Root/`。

- `{PROJECT_ROOT}`：Runner 指定的測試專案 root
- `{MATERIAL_ROOT}`：正式 source / BlockDocs / Issues / DDL / Scheduler evidence
- `{CONFIG_ROOT}`：mapping / validation / prompt
- `{TOOLS_ROOT}`：validator tools
- `{PROJECT_CONTRACT}`：測試契約

開始前先讀 `{PROJECT_CONTRACT}`。

## Output

```text
TokenGenerate_CommandSubmit/
  TokenGenerate_CommandSubmit.vb
  Workflow/
    Create SOP.sql
    Create Condition.sql
    Create Action.sql
    Validation.sql
  TokenGenerate_CommandSubmit-SOP-001/
    prepare.sql
    mock_<external>.*   # only when required
```

Case 數量不固定。

`Workflow/` 是本 E2E target 自己的 workflow SQL，因此單積木與多積木 workflow 可以不同。這四份 SQL 是 documentation/spec SQL，不由 `RunPrepareSql` 執行：
- `Create SOP.sql`
- `Create Condition.sql`
- `Create Action.sql`
- `Validation.sql`

`Global/` 只允許真正跨所有 E2E target 共用且與特定 workflow 無關的 SQL；不得把本 target 的 workflow definition 放到 Global。

## SOP Fixture Package

每個 `TokenGenerate_CommandSubmit-SOP-NNN/` 是獨立 Case：
- `prepare.sql`：只允許 INSERT before-state。
- `mock_*`：只允許真正 external boundary，例如 API/HTTP、MQ、Kafka、NATS、WSDL/SOAP、gRPC、第三方 event/payload。

Fixture 必須由自己的 TestMethod 讀取與註冊，不可跨 SOP 共用，也不可在 VB hardcode payload。

可使用 TestInfrastructure：
- `UseApiMock(...)`
- `UseMqMock(...)`
- `UseKafkaMock(...)`
- `UseNatsMock(...)`
- `UseGrpcMock(...)`
- `UseWsdlMock(...)`
- 或 generic `UseExternalMock(...)`

DB / Repository / Business / Block / Workflow / Main / mapped Entry/Critical Function 都不可 mock。

## UnitTest execution

每個 TestMethod：
1. optional external mock setup（僅真的需要時）
2. `RunPrepareSql(...)`
3. 設定合法 runtime/env input
4. 只呼叫 `CallMain("TokenGenerate_CommandSubmit")`
5. `Assert.*` observable post-state

不可直接 call `Block.Execute` / Business / Repository / Entry/Critical。

每個 TestMethod 使用可讀中文 DisplayName，但 Python Structure Gate 不解析 VB 語法；這項由 AI Grill/Review 檢查。

## Workflow SQL

Workflow SQL 必須描述完整 composite workflow，且 Action/SOP 順序必須是：TokenGenerate -> CommandSubmit。Validation.sql 必須驗證整條 workflow 的 post-state。

Workflow SQL 不得製造 Case 的 expected result。Case-specific before-state 一律放自己的 `prepare.sql`。

## Coverage

Coverage 只由 `{CONFIG_ROOT}/function_mapping.json` 決定。Composite target 的所有 `entry_functions` 與 `critical_functions` 必須在同一次 E2E validation 中各自 > 90%，不可拆成兩個 block 各自驗證。

不可修改 `{MATERIAL_ROOT}`、`{CONFIG_ROOT}/validation.py`、`{CONFIG_ROOT}/function_mapping.json`、`{CONFIG_ROOT}/ai_validator.template.md`、`{TOOLS_ROOT}`、`TestProject.vbproj`、`TestInfrastructure/`。

完成前執行 task context 指定 validator command，並傳入 `--block TokenGenerate_CommandSubmit`。

## Planning

優先讀：
1. PROJECT_CONTRACT.md
2. `{MATERIAL_ROOT}/BlockDocs/TokenGenerate_CommandSubmit.md`
3. Issues/*
4. DDL/*
5. Scheduler/*
6. function_mapping.json
7. mapped Entry/Critical source
8. Main/Workflow source

能判斷 meaningful E2E behavior 後立即建立 TODO，不要掃整個 solution。
