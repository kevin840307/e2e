# HoldLot

Entry: HoldLotBlock.Execute

Internal flow:

```text
HoldLotBlock.Execute
  -> RouteRepository.QueryLotState
  -> RouteRepository.QueryHoldPolicy
  -> RouteRepository.InsertHold
```

Branches:
- missing `LOT_STATE` -> `HoldLot.Result=LOT_NOT_FOUND`;
- missing or disabled `HOLD_POLICY` -> `HoldLot.Result=HOLD_NOT_ALLOWED`;
- allowed policy -> writes `LOT_HOLD` and sets `HoldLot.Result=HELD`.

Fixture implications:
- `E2E_SOP_CONTROL` drives which SOP is executed.
- `LOT_STATE` is runtime before-state for the lot under test.
- `HOLD_POLICY` is parameter/config data and may live in `E2E_PARAM_DB_ROOT`.
- `LOT_HOLD` is production output and must not be pre-seeded by fixture SQL.
- A realistic SOP may need `prepare.sql` for control/runtime state and `prepare.params.sql` for hold policy.
