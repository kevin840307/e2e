# AI Grill + Review

Review only. Do not modify files.

把 `--block` 視為 `function_mapping.json` 定義的 E2E target。

確認：
- Case 是在測 target 本身，不是 AI 自行隨機串接 block；
- single target 只測自己的 Action/Condition SOP；
- composite 只照 `workflow_blocks` 指定順序執行，且不可拆開；
- 可共用的 Workflow/SOP SQL reuse `Global/Workflow/`，沒有每個 Case 重複建立；
- 每個 SOP 的差異只在自己的 `prepare.sql + mock_*`；
- `prepare.sql` 只有 before-state INSERT，沒有預造 result/audit/history/final state；
- mock 只用於 API/HTTP、MQ、Kafka、NATS、WSDL/SOAP、gRPC 等 external boundary；
- DB/Repository/Business/Block/Workflow/Main/mapped function 沒有被 mock；
- Test flow 是 `[mock] -> prepare.sql -> CallMain(target) -> Assert`；
- 每個 SOP 獨立，沒有跨 SOP state；
- build/test/TRX 成功；
- 每個 mapped `entry_functions + critical_functions` 唯一解析且 `> min_coverage`。

只回報具體問題與需要修正的證據，不替 runner 做最終 pass/fail。
