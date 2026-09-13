# RouteDispatch
Entry: RouteDispatchBlock.Execute

Important internal functions:
- RouteDecisionService.ResolveRoute
- RouteRepository.QueryRoutePolicy
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
