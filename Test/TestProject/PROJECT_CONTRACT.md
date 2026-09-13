# E2E Regression UnitTest Contract

`Test/TestProject` 是 AI Task Runner project-root 與 E2E output root。

## E2E Target

`config/function_mapping.json` 的 key 是 **E2E target**，不一定等於單一 Block。

- `workflow_blocks` 只有一個：single-block E2E。
- `workflow_blocks` 有多個：Composite E2E；這些 block 必須由同一次 Main/Workflow、同一個 WorkflowContext 一起測，禁止拆成獨立 E2E。

例如：

```text
TokenGenerate_CommandSubmit
  = TokenGenerate -> CommandSubmit
```

## Output

每個 target：

```text
<TARGET>/
  <TARGET>.vb
  Workflow/
    Create SOP.sql
    Create Condition.sql
    Create Action.sql
    Validation.sql
  <TARGET>-SOP-001/
    prepare.sql
    mock_<external>.*   # optional
```

`Workflow/*.sql` 是 target-owned documentation/spec SQL，不由 `RunPrepareSql` 執行。Composite target 的 workflow SQL 必須包含全部 blocks 與正確順序。

`Global/` 只放真正跨 target 共用、且與特定 workflow definition 無關的資料；不得把 target-specific workflow SQL 放 Global。

## UnitTest execution

每個 TestMethod 固定：

```text
[optional external mock]
-> own prepare.sql
-> CallMain("<TARGET>")
-> Assert observable production post-state
```

External boundary 可 Mock：API/HTTP、MQ、Kafka、NATS、WSDL/SOAP、gRPC、第三方服務/event。

TestInfrastructure 提供：
- `UseApiMock`
- `UseMqMock`
- `UseKafkaMock`
- `UseNatsMock`
- `UseGrpcMock`
- `UseWsdlMock`
- generic `UseExternalMock`

所有 fixture 必須在自己的 SOP folder，命名 `mock_*`，不可跨 SOP 共用。

不可 mock：SQL/DB、Repository、Business、Block、Workflow、Main、Entry/Critical function。

`prepare.sql` 只建立 before-state，只允許 INSERT；不可預建 Expected/Result/Audit/History/Final State。

## Workflow dependency

Composite E2E 必須共用同一個 runtime DB、WorkflowContext 與 Main call。不得為了 coverage 直接呼叫其中任一 Block.Execute。

`TokenGenerate_CommandSubmit` 的正式順序：

```text
TokenGenerate
-> context[Command.Token]
-> CommandSubmit
-> external MQ
```

## Coverage

Coverage targets 只讀 `config/function_mapping.json`。

- single target：驗證其 `entry_functions + critical_functions`
- composite target：同一次 validation 一起驗證全部 blocks 的 `entry_functions + critical_functions`
- 每個 mapped function 都必須嚴格 `> min_coverage`

## Structure Gate

Python StructureChecker 只確認 filename/folder：
- `<TARGET>/<TARGET>.vb`
- `<TARGET>/Workflow/` 四個固定 SQL filename
- 至少一個 `<TARGET>-SOP-NNN/prepare.sql`
- SOP 只允許 `prepare.sql` 與 `mock_*`

Python 不解析 VB TestMethod、DisplayName、CallMain 語意；這些由 AI Grill/Review 判斷。

## Parameterized SQL

`prepare.sql` 可使用 `{{PARAM_NAME}}`，由 `SqlParams(...)` 傳入。只參數化 value，不參數化 identifier；missing/unused parameter fail-fast。

## Protected inputs

AI 只能在 TestProject output 範圍建立 target artifacts。不得修改 material、mapping、validator、tools、TestInfrastructure、TestProject.vbproj。
