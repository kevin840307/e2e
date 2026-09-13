請產生 `{{BLOCK}}` E2E Regression Test。

## Target

- E2E target：`{{BLOCK}}`
- Workflow type：`{{WORKFLOW_TYPE}}`
- Production blocks：`{{WORKFLOW_BLOCKS}}`
- External mocks：`{{EXTERNAL_MOCKS}}`

{{WORKFLOW_RULE}}

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
{{BLOCK}}/
  {{BLOCK}}.vb
  {{BLOCK}}-SOP-001/
    prepare.sql
    prepare.params.sql       # optional: block/SOP parameter tables
    prepare.<db-name>.sql    # optional: when before-state belongs to another DB/root
    mock_<external>.*   # optional
```

- `prepare*.sql`：必須先用 control table 啟動目標 SOP，再準備 case-specific DB before-state。
  - 啟動列只能使用 `UPSERT_STATUS INTO E2E_SOP_CONTROL (TARGET, SOP, STATUS) VALUES (...) KEY (TARGET, SOP)`；若資料已存在，只能更新 `STATUS`。
  - 其他 before-state 只能使用 `INSERT_IF_NOT_EXISTS INTO <TABLE> (...) VALUES (...) KEY (...)`；若 key 已存在，不可更新、不覆蓋、不刪除。
  - 禁止 `DELETE`、一般 `UPDATE`、`MERGE`，也禁止用 fixture 改 production output table。
  - 新增 fixture 前先檢查目前專案既有測試資料；若 DB 內可能已有相同 key 且內容會與本 case 衝突，換一組不衝突的 lotId / eqId / product / correlationId 等資料。
  - 不只補 block parameters；只要 SOP 流程會查詢的資料、跨 block 依賴資料、routing/recipe/policy/config/master data、或讓流程走到指定分支所需的資料，都要以同樣規則準備。
  - 若資料分屬不同 DB/root/schema，拆成多個 `prepare*.sql`，TestMethod 依序呼叫各自的 `RunPrepareSql(...)`；每個 SQL 檔只處理自己的 DB/root，不要把不同 DB 的資料混在同一檔。
  - `prepare.sql` 保留作為本 SOP 的主啟動/主 DB fixture；參數表可用 `prepare.params.sql`，其他 DB 可用 `prepare.<db-name>.sql`。
- Composite target 也必須準備完整依賴資料，不可只準備第一個 block。像 `TokenGenerate_CommandSubmit` 這類 target，必須同時考慮 TokenGenerate 的參數/查表資料、CommandSubmit 的參數/查表資料、兩者交接所需的 context/DB dependency，以及 MQ boundary fixture；但 production output table 仍不可預先造資料。
- `mock_*`：只模擬真正 external boundary，例如 API/HTTP、MQ、Kafka、NATS、WSDL/SOAP、gRPC。
- 每個 SOP 必須獨立，不可依賴其他 SOP。
- DB / Repository / Business / Block / Workflow / Main / mapped functions 不可 mock。

### 3. Test flow

每個 TestMethod 固定：

```text
optional mock setup
-> own prepare*.sql, one call per DB/root when needed
-> runtime/env parameters
-> CallMain()
-> Assert observable post-state
```

不可直接呼叫 Block.Execute / Business / Repository 來製造 coverage。

## Coverage

Coverage 只看 `function_mapping.json`。
Composite target 的全部 `entry_functions + critical_functions` 必須在同一次 validation 各自 `> min_coverage`。

## Protected

不可修改 material、mapping、validator、tools、TestInfrastructure、Test/*.vbproj、PROJECT_CONTRACT.md。

完成前執行 task context 指定 validator，並傳入 `--block {{BLOCK}}`。
