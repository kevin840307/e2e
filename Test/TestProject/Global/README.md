# Global shared artifacts

`Global/` is only for artifacts that are genuinely shared by every E2E target and are not part of a target-specific workflow definition.

Target-specific workflow SQL belongs under:

```text
<TARGET>/Workflow/
  Create SOP.sql
  Create Condition.sql
  Create Action.sql
  Validation.sql
```

This is required because single-block and composite workflows have different SOP/Condition/Action definitions.
