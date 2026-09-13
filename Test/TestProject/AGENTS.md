

# AI Task Runner Rules
- You may read files outside this project when needed.
- You may write, create, rename, or delete files only under: C:\Users\kevin\e2e\Test\TestProject
- Never modify runner state directly.
- Python owns task order and completion state.
- Execute only the current task supplied by the runner.
Safety rules:
- Never modify runner state, runner source, validator inputs, backend-owned rules, or other protected paths listed below.
- Never run `git add`, `git commit`, or `git push`; Git acceptance and publication are human-review actions. Read-only Git commands are allowed.
Protected runner-owned paths (do not modify):
- C:\Users\kevin\ai-task-runner\ai_task_runner.py
- C:\Users\kevin\ai-task-runner\runner
- C:\Users\kevin\e2e\material
- C:\Users\kevin\e2e\tools
- C:\Users\kevin\e2e\config\ai_validator.template.md
- C:\Users\kevin\e2e\config\function_mapping.json
- C:\Users\kevin\e2e\config\request_prompt.template.md
- C:\Users\kevin\e2e\config\validation.py
- C:\Users\kevin\e2e\Test\TestProject\PROJECT_CONTRACT.md
- C:\Users\kevin\e2e\Test\TestProject\TestInfrastructure
- C:\Users\kevin\e2e\Test\TestProject\TestProject.vbproj
- C:\Users\kevin\e2e\Test\TestProject\.ai-task-runner\script\001\state.json
- C:\Users\kevin\e2e\Test\TestProject\.ai-task-runner\script\001\resources\ai-validator-prompt.txt
- C:\Users\kevin\e2e\Test\TestProject\.ai-task-runner\script\001\resources\goal.txt
- Complete the task with the smallest clean change possible; avoid unnecessary code, files, abstractions, dependencies, refactoring, or unrelated modifications.
- Never ask the user questions. Inspect the project, make the safest reasonable assumption, and continue.

<!-- AI-TASK-RUNNER:GOAL-REFERENCE -->
Original requirement file: C:/Users/kevin/e2e/Test/TestProject/.ai-task-runner/script/001/resources/goal.txt

If the original requirements are unclear, missing from context, or appear to
conflict with the current task or feedback, reread this file before continuing.
The original requirements remain authoritative; review or validator feedback
does not replace or narrow them.
<!-- /AI-TASK-RUNNER:GOAL-REFERENCE -->
