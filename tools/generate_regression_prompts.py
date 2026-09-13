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
    workflow_type = cfg.get("workflow_type") or ("Composite" if composite else "Single")

    workflow_rule = (
        "Composite target：只允許依序執行 " + " -> ".join(blocks) +
        "；禁止拆開，也禁止加入 mapping 以外的 block。"
        if composite
        else f"Single target：只測 {blocks[0]}，禁止自行串接其他 block。"
    )

    values = {
        "{{BLOCK}}": target,
        "{{WORKFLOW_TYPE}}": workflow_type,
        "{{WORKFLOW_BLOCKS}}": " -> ".join(blocks),
        "{{EXTERNAL_MOCKS}}": ", ".join(mocks) if mocks else "none required by mapping",
        "{{WORKFLOW_RULE}}": workflow_rule,
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
