請產生 `RouteDispatch` E2E Regression Test。

## Target

- E2E target：`RouteDispatch`
- Workflow type：`Action`
- Production blocks：`RouteDispatch`
- External mocks：`none required by mapping`

Single target：只測 RouteDispatch，禁止自行串接其他 block。

先讀 `PROJECT_CONTRACT.md`、BlockDocs、function_mapping.json 與 mapped source；足以判斷 Case 後就開始，不要掃完整 solution。

## 核心規則

### 1. Workflow/SOP topology

測試的是 **mapping 指定的 target**，不是讓 AI 自由設計 Workflow。

- single block：只建立/使用該 block 的 Action 或 Condition SOP。
- composite：只照 `workflow_blocks` 指定順序串接。
- 禁止為了 coverage 隨機加入其他 block。
- 禁止把 composite 拆成獨立 E2E。

建立 Workflow/SOP 的 SQL 若可共用，優先 reuse `Global/Workflow/`；不要每個 Case 複製一份。
只有 mapping 明確為 composite，且 Global template 無法表示時，才允許 target-specific Workflow SQL。

### 2. Case fixture

每個 Case 只放自己的差異：

```text
RouteDispatch/
  RouteDispatch.vb
  RouteDispatch-SOP-001/
    prepare.sql
    mock_<external>.*   # optional
```

- `prepare.sql`：block parameters + DB before-state，只允許 INSERT。
- `mock_*`：只模擬真正 external boundary，例如 API/HTTP、MQ、Kafka、NATS、WSDL/SOAP、gRPC。
- 每個 SOP 必須獨立，不可依賴其他 SOP。
- DB / Repository / Business / Block / Workflow / Main / mapped functions 不可 mock。

### 3. Test flow

每個 TestMethod 固定：

```text
optional mock setup
-> own prepare.sql
-> runtime/env parameters
-> CallMain("RouteDispatch")
-> Assert observable post-state
```

不可直接呼叫 Block.Execute / Business / Repository 來製造 coverage。

## Coverage

Coverage 只看 `function_mapping.json`。
Composite target 的全部 `entry_functions + critical_functions` 必須在同一次 validation 各自 `> min_coverage`。

## Protected

不可修改 material、mapping、validator、tools、TestInfrastructure、Test/TestProject/*.vbproj、PROJECT_CONTRACT.md。

完成前執行 task context 指定 validator，並傳入 `--block RouteDispatch`。
