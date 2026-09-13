# Block model

Most blocks are independent and may be placed in any order.

Independent examples:
- RouteDispatch
- HoldLot
- EquipmentCheck
- AlarmNotify

Exception:
- TokenGenerate -> CommandSubmit
  CommandSubmit consumes `Command.Token` produced by TokenGenerate.

AI must not assume a dependency unless Source or documentation explicitly proves it.
