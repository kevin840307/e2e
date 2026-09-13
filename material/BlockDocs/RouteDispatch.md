# RouteDispatch
Entry: RouteDispatchBlock.Execute

Important internal functions:
- RouteDecisionService.ResolveRoute
- RouteDecisionService.ApplyDispatchParameters
- RouteRepository.QueryRoutePolicy
- RouteRepository.QueryDispatchParam
- RouteRepository.QueryCondition
- RouteDecisionService.ShouldHoldForRecipe
- RouteDecisionService.IsQueueFull

Branches:
- no policy -> ERROR
- main route
- sub route by condition
- force main
- force sub
- recipe missing -> HOLD
- queue full -> HOLD
- normal -> DISPATCH

Fixture implications:
- `E2E_SOP_CONTROL` drives which SOP is executed.
- `ROUTE_DISPATCH_PARAM` is parameter/config data and may live in `E2E_PARAM_DB_ROOT`.
- Route decision may query `ROUTE_POLICY`, `ROUTE_CONDITION`, `RECIPE_MAPPING`, and `LOT_QUEUE`.
- A realistic SOP may need several `prepare*.sql` files: control/runtime DB, parameter DB, and route/master data.
