# EquipmentCheck
Entry: EquipmentCheckBlock.Execute
Independent block.

Business:
- EquipmentService.Evaluate
- EquipmentService.IsUnderMaintenance
- EquipmentService.IsBlockedByRule
Repository:
- EquipmentRepository.QueryEquipment
- EquipmentRepository.QueryEquipmentRule
- EquipmentRepository.QueryMaintenanceWindow

Results:
- NOT_FOUND
- MAINTENANCE
- NOT_RUN
- MODE_BLOCKED
- MODE_MISMATCH
- READY

Fixture implications:
- `E2E_SOP_CONTROL` drives which SOP is executed.
- Equipment state comes from `EQUIPMENT`.
- Parameter/config data may come from `EQUIPMENT_RULE` and `EQUIPMENT_MAINTENANCE`, commonly in `E2E_PARAM_DB_ROOT`.
- SOP SQL should prepare all queried tables needed to drive the desired branch, using status-only upsert for control rows and insert-if-not-exists for other rows.
