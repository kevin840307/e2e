"""Generate E2E prompts and regression YAML from function_mapping.json."""

from __future__ import annotations

import argparse
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
CONFIG = ROOT / "config"
TEST = ROOT / "Test" / "TestProject"
DEFAULT_OUTPUT = CONFIG / "prompts" / "regression_all"


def render_prompt(template: str, target: str, cfg: dict) -> str:
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


def q(value: Path | str) -> str:
    return "'" + str(value).replace("'", "''") + "'"


def find_test_project() -> Path:
    projects = sorted(TEST.glob("*.vbproj"))
    if len(projects) != 1:
        names = ", ".join(p.name for p in projects) or "none"
        raise SystemExit(f"Expected exactly one *.vbproj under {TEST}; found: {names}")
    return projects[0]


def render_regression(targets: list[str], *, hard: bool, project: Path) -> str:
    ai_count = 3 if hard else 1
    required = 3 if hard else 1
    review_retries = 3 if hard else 1

    protected = [
        ROOT / "material",
        CONFIG / "validation.py",
        CONFIG / "function_mapping.json",
        CONFIG / "ai_validator.template.md",
        CONFIG / "request_prompt.template.md",
        ROOT / "tools",
        TEST / "TestInfrastructure",
        project,
        TEST / "PROJECT_CONTRACT.md",
    ]

    items = []
    for target in targets:
        lines = [
            "- project_root: .",
            f"  goal_file: {q(DEFAULT_OUTPUT / (target + '.md'))}",
            f"  validator: {q(CONFIG / 'validation.py')}",
            f"  validator_args: [--block, {target}]",
            f"  ai_validator_prompt_file: {q(CONFIG / 'ai_validator.template.md')}",
            f"  ai_validator_count: {ai_count}",
            f"  ai_validator_required_passes: {required}",
            "  protect_files:",
        ]
        lines.extend(f"    - {q(path)}" for path in protected)
        lines.extend([
            "  max_attempts: 2",
            f"  review_retries: {review_retries}",
            "  max_cycles: 20",
        ])
        items.append("\n".join(lines))
    return "\n\n".join(items) + "\n"


def grouped_targets(mapping: dict) -> dict[str, list[str]]:
    groups: dict[str, list[str]] = {}
    for target, cfg in mapping.items():
        group = cfg.get("group")
        if not group:
            continue
        name = re.sub(r"[^A-Za-z0-9_-]+", "_", str(group)).strip("_")
        if name:
            groups.setdefault(name, []).append(target)
    return groups


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--output-dir", type=Path, default=DEFAULT_OUTPUT)
    parser.add_argument("--block", action="append", dest="blocks")
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()

    mapping = json.loads((CONFIG / "function_mapping.json").read_text(encoding="utf-8"))
    all_targets = list(mapping)
    targets = args.blocks or all_targets

    unknown = sorted(set(targets) - set(mapping))
    if unknown:
        parser.error("unknown target(s): " + ", ".join(unknown))

    template = (CONFIG / "request_prompt.template.md").read_text(encoding="utf-8")
    changed = False

    for target in targets:
        changed |= write_if_changed(
            args.output_dir / f"{target}.md",
            render_prompt(template, target, mapping[target]),
            args.check,
        )

    project = find_test_project()

    changed |= write_if_changed(
        CONFIG / "regression_all.yaml",
        render_regression(all_targets, hard=False, project=project),
        args.check,
    )
    changed |= write_if_changed(
        CONFIG / "regression_all_hard.yaml",
        render_regression(all_targets, hard=True, project=project),
        args.check,
    )

    # Optional: add "group": "command" etc. to mapping entries.
    for group, group_items in grouped_targets(mapping).items():
        changed |= write_if_changed(
            CONFIG / f"regression_group_{group}.yaml",
            render_regression(group_items, hard=False, project=project),
            args.check,
        )
        changed |= write_if_changed(
            CONFIG / f"regression_group_{group}_hard.yaml",
            render_regression(group_items, hard=True, project=project),
            args.check,
        )

    return 1 if args.check and changed else 0


if __name__ == "__main__":
    raise SystemExit(main())
