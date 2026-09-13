# Known issues
- RouteDispatch: MAX_QUEUE=0 means unlimited.
- RouteDispatch: FORCE_MAIN overrides USE_SUB_ROUTE condition.
- EquipmentCheck: missing equipment row must not be treated as READY.
- AlarmNotify: HTTP 500 is allowed to return as result; block must not fake success.
- CommandSubmit: direct execution without TokenGenerate must return INVALID_TOKEN.
