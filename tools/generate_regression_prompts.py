"""Render E2E-target prompts from function_mapping.json and the shared template."""

from __future__ import annotations

import argparse
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
CONFIG = ROOT / "config"
DEFAULT_OUTPUT = CONFIG / "prompts" / "regression_all"


def render(template: str, target: str, cfg: dict) -> str:
    blocks = cfg.get("workflow_blocks") or [target]
    mocks = cfg.get("external_mocks") or []
    composite = len(blocks) > 1

    workflow_rule = (
        "這是 Composite E2E。Main 必須建立同一個 WorkflowContext，並依序執行："
        + " -> ".join(blocks)
        + "。禁止為這些積木建立獨立 E2E target，也禁止只測其中一個。"
        if composite
        else f"這是 single-block E2E。Main 的 workflow 只包含 {blocks[0]}。"
    )
    workflow_sql_rule = (
        "Workflow SQL 必須描述完整 composite workflow，且 Action/SOP 順序必須是："
        + " -> ".join(blocks)
        + "。Validation.sql 必須驗證整條 workflow 的 post-state。"
        if composite
        else f"Workflow SQL 只描述 {blocks[0]} 的 workflow definition。"
    )

    values = {
        "{{BLOCK}}": target,
        "{{WORKFLOW_BLOCKS}}": " -> ".join(blocks),
        "{{EXTERNAL_MOCKS}}": ", ".join(mocks) if mocks else "none required by mapping",
        "{{WORKFLOW_RULE}}": workflow_rule,
        "{{WORKFLOW_SQL_RULE}}": workflow_sql_rule,
    }
    for key, value in values.items():
        template = template.replace(key, value)
    return template


def write_if_changed(path: Path, content: str, check: bool) -> bool:
    current = path.read_text(encoding="utf-8") if path.is_file() else None
    if current == content:
        return False
    if check:
        print(f"OUTDATED {path}")
        return True
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding="utf-8")
    print(f"WROTE {path}")
    return True


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--output-dir", type=Path, default=DEFAULT_OUTPUT)
    parser.add_argument("--block", action="append", dest="blocks")
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()

    mapping = json.loads((CONFIG / "function_mapping.json").read_text(encoding="utf-8"))
    targets = args.blocks or list(mapping)
    unknown = sorted(set(targets) - set(mapping))
    if unknown:
        parser.error("unknown target(s): " + ", ".join(unknown))

    task_template = (CONFIG / "request_prompt.template.md").read_text(encoding="utf-8")
    changed = False
    for target in targets:
        changed |= write_if_changed(
            args.output_dir / f"{target}.md",
            render(task_template, target, mapping[target]),
            args.check,
        )

    return 1 if args.check and changed else 0


if __name__ == "__main__":
    raise SystemExit(main())
