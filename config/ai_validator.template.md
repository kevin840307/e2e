# AI Grill + Review

Review only. Do not modify files.

把 `--block` 視為 `function_mapping.json` 定義的 E2E target。

你可以自行執行 build、測試、coverage 計算、TRX/log 檢查等唯讀驗證命令來確認結果；不要只靠閱讀檔案或模型主觀判斷。

確認：
- Case 是在測 target 本身，不是 AI 自行隨機串接 block；
- single target 只測自己的 Action/Condition SOP；
- composite 只照 `workflow_blocks` 指定順序執行，且不可拆開；
- 可共用的 Workflow/SOP SQL reuse `Global/Workflow/`，沒有每個 Case 重複建立；
- 每個 SOP 的差異只在自己的 `prepare*.sql + mock_*`；
- `prepare*.sql` 先用 status/control table 啟動目標 SOP；該列若已存在只能更新 `STATUS`；
- 其他 before-state 資料若 key 已存在不可更新或覆蓋，只能在不存在時新增；
- `prepare*.sql` 包含 block parameters、流程查表資料、跨 block 依賴資料、routing/recipe/policy/config/master data 等必要 before-state，不只是一張參數表；
- 多 DB/root/schema 的 before-state 有拆成多個 `prepare*.sql`，且 TestMethod 依序執行對應 SQL；
- Composite target 有準備所有 mapped blocks 所需的參數與依賴資料，例如 `TokenGenerate_CommandSubmit` 不只準備 TokenGenerate，也準備 CommandSubmit 查表/參數/MQ fixture 需求；
- `prepare*.sql` 沒有 DELETE、一般 UPDATE、MERGE，也沒有預造 result/audit/history/final state；
- mock 只用於 API/HTTP、MQ、Kafka、NATS、WSDL/SOAP、gRPC 等 external boundary；
- DB/Repository/Business/Block/Workflow/Main/mapped function 沒有被 mock；
- Test flow 是 `[mock] -> prepare*.sql -> CallMain() -> Assert`，target/SOP 由 Main 內部讀 control table 決定；
- 每個 SOP 獨立，沒有跨 SOP state；
- build/test/TRX 成功；
- 每個 mapped `entry_functions + critical_functions` 唯一解析且 `> min_coverage`。

只回報具體問題與需要修正的證據，不替 runner 做最終 pass/fail。
