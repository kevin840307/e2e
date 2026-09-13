from __future__ import annotations

import argparse
import json
import locale
import re
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
CONFIG = ROOT / "config"
TEST = ROOT / "Test" / "TestProject"
TOOLS = ROOT / "tools"
MAPPING = {}


def configure_paths(project_root=""):
    global ROOT, CONFIG, TEST, TOOLS, MAPPING

    script_root = Path(__file__).resolve().parent.parent
    requested = Path(project_root).resolve() if project_root else script_root

    if (requested / "TestProject.vbproj").is_file():
        TEST = requested
        ROOT = requested.parent.parent if requested.parent.name.lower() == "test" else requested.parent
    else:
        ROOT = requested
        TEST = ROOT / "Test" / "TestProject"

    CONFIG = ROOT / "config"
    TOOLS = ROOT / "tools"

    for path in (str(TOOLS), str(script_root / "tools")):
        if path not in sys.path:
            sys.path.insert(0, path)

    MAPPING = json.loads((CONFIG / "function_mapping.json").read_text(encoding="utf-8"))


configure_paths()

from ai_task_runner_validator import ValidatorReport
from coverage_parser import parse_coverage, resolve, candidates

def read(path):
    return path.read_text(encoding="utf-8-sig", errors="replace")


def decode(data):
    if not data:
        return ""
    for enc in (locale.getpreferredencoding(False), "utf-8", "cp950", "big5"):
        try:
            return data.decode(enc)
        except Exception:
            pass
    return data.decode("utf-8", errors="replace")


def test_display_names(block):
    source = TEST / block / f"{block}.vb"
    if not source.is_file():
        return {}

    pattern = re.compile(
        r'<\s*TestMethod\s*\(\s*"(?P<display>[^"]+)"\s*\)\s*>\s*'
        r'(?:Public|Friend)\s+Sub\s+(?P<method>\w+)',
        re.I,
    )
    return {
        match.group("method"): match.group("display").strip()
        for match in pattern.finditer(read(source))
    }


def test_results(report_dir, display_names):
    trx = report_dir / "test-results.trx"
    if not trx.is_file():
        return []

    try:
        root = ET.parse(trx).getroot()
    except ET.ParseError:
        return []

    methods_by_test_id = {}
    for node in root.iter():
        if node.tag.rsplit("}", 1)[-1] != "UnitTest":
            continue
        method = next(
            (
                child.get("name", "")
                for child in node.iter()
                if child.tag.rsplit("}", 1)[-1] == "TestMethod"
            ),
            "",
        )
        methods_by_test_id[node.get("id", "")] = method

    results = []
    for node in root.iter():
        if node.tag.rsplit("}", 1)[-1] != "UnitTestResult":
            continue
        name = node.get("testName", "unknown")
        method = methods_by_test_id.get(node.get("testId", ""), "")
        method = method or name.rsplit(".", 1)[-1]
        results.append((
            node.get("outcome", "Unknown"),
            method,
            display_names.get(method, name),
            node.get("duration", ""),
        ))
    return results


def markdown_cell(value):
    return str(value).replace("|", "\\|").replace("\r", " ").replace("\n", " ")


def write_review_markdown(block, report):
    report_dir = report.report_dir
    result_dir = TEST / "result"
    status = "PASS" if not report.errors else "FAIL"
    lines = [
        f"# {block} Test Review",
        "",
        f"- Validation: **{status}**",
        f"- Report directory: `{report_dir}`",
        "",
        "## Test Results",
        "",
        "| Result | Test method | Description | Duration |",
        "| --- | --- | --- | --- |",
    ]

    results = test_results(report_dir, test_display_names(block))
    if results:
        lines.extend(
            "| " + " | ".join(markdown_cell(value) for value in result) + " |"
            for result in results
        )
    else:
        lines.append("| Not run | - | No TRX test result was produced. | - |")

    coverage = report_dir / "coverage.txt"
    lines.extend(["", "## Coverage", ""])
    if coverage.is_file():
        lines.extend(f"- {line}" for line in read(coverage).splitlines())
    else:
        lines.append("- Not available.")

    lines.extend(["", "## Evidence", ""])
    for name in ("restore.log", "build.log", "test.log", "coverage.cobertura.xml"):
        if (report_dir / name).is_file():
            lines.append(f"- `{name}`")
    if report.errors:
        lines.extend(["", "## Validation Errors", ""])
        lines.extend(f"- `{item.code}`: {item.title}" for item in report.errors)

    result_dir.mkdir(parents=True, exist_ok=True)
    (result_dir / f"{block}.md").write_text(
        "\n".join(lines) + "\n",
        encoding="utf-8",
    )


def update_project_review():
    root = TEST / "result"
    reviews = sorted(path for path in root.glob("*.md") if path.name != "README.md")
    lines = [
        "# E2E Test Review",
        "",
        "Generated by `validation.py`. Each section is the latest validation result for one block.",
    ]
    for review in reviews:
        lines.extend(["", "---", "", read(review).strip()])
    (root / "README.md").write_text("\n".join(lines) + "\n", encoding="utf-8")


class Checker:
    def __init__(self, block, report, info):
        self.block = block
        self.cfg = MAPPING[block]
        self.report = report
        self.info = info

    def error(self, code, title, *details, fix="", report_name=None, report_content=None):
        self.report.error(
            code,
            title,
            details,
            fix,
            report_name=report_name,
            report_content=report_content,
        )

    def ok(self, message):
        self.info.append(message)


class StructureChecker(Checker):
    def run(self):
        folder = TEST / self.block
        vb = folder / f"{self.block}.vb"

        if not folder.is_dir():
            self.error("STRUCTURE_BLOCK", "Block folder missing", folder)
            return

        if not vb.is_file():
            self.error("STRUCTURE_FILE", "UnitTest file missing", vb)
        else:
            for match in re.finditer(
                r"<\s*TestMethod(?P<args>\s*\([^>]*\))?\s*>",
                read(vb),
                re.I,
            ):
                args = match.group("args") or ""
                if not re.fullmatch(r'\s*\(\s*"[^"\r\n]+"\s*\)\s*', args):
                    self.error(
                        "STRUCTURE_TEST_NAME",
                        "TestMethod requires a non-empty display name",
                        vb,
                        fix='Use <TestMethod("測試目的、條件與預期結果")>.',
                    )

        sops = sorted(
            p for p in folder.iterdir()
            if p.is_dir() and re.fullmatch(
                re.escape(self.block) + r"-SOP-\d+",
                p.name,
                re.I,
            )
        )

        if not sops:
            self.error("STRUCTURE_SOP", "At least one SOP folder is required", folder)

        for sop in sops:
            prepare = sop / "prepare.sql"
            if not prepare.is_file():
                self.error("STRUCTURE_PREPARE", "prepare.sql missing", prepare)

            for item in sop.iterdir():
                if item.is_dir():
                    self.error("STRUCTURE_NESTED", "Nested folder is not allowed in SOP", item)
                    continue

                name = item.name
                if name.lower() == "prepare.sql":
                    continue

                if re.fullmatch(r"mock_[A-Za-z0-9_.-]+", name, re.I):
                    continue

                self.error(
                    "STRUCTURE_ARTIFACT",
                    "Unsupported SOP filename",
                    item,
                    fix="Only prepare.sql and mock_* files are allowed in a SOP folder.",
                )

        if not any(x.code.startswith("STRUCTURE_") for x in self.report.errors):
            self.ok(f"structure PASS: {len(sops)} SOP(s)")


class CheatChecker(Checker):
    def run(self):
        folder = TEST / self.block

        for sop in folder.glob(f"{self.block}-SOP-*"):
            if not sop.is_dir():
                continue

            prepare = sop / "prepare.sql"
            if not prepare.is_file():
                continue

            sql = re.sub(r"--[^\r\n]*", "", read(prepare))
            statements = [x.strip() for x in sql.split(";") if x.strip()]

            for stmt in statements:
                if not re.match(r"^INSERT\s+INTO\b", stmt, re.I | re.S):
                    self.error(
                        "CHEAT_SQL",
                        "prepare.sql may only create before-state with INSERT",
                        prepare,
                    )

            for table in self.cfg.get("forbidden_prepare_tables", []):
                if re.search(r"\bINSERT\s+INTO\s+" + re.escape(table) + r"\b", sql, re.I):
                    self.error(
                        "CHEAT_RESULT",
                        "prepare.sql manufactures production result",
                        table,
                    )

            for item in sop.iterdir():
                if (
                    item.is_file()
                    and item.name.lower() != "prepare.sql"
                    and re.fullmatch(r"mock_[A-Za-z0-9_.-]+", item.name, re.I)
                ):
                    payload = read(item)
                    if re.search(
                        r"\b(INSERT\s+INTO|UPDATE|DELETE\s+FROM|MERGE\s+INTO)\b",
                        payload,
                        re.I,
                    ):
                        self.error(
                            "CHEAT_MOCK_DB",
                            "External mock may not mutate DB",
                            item,
                        )

        if not any(x.code.startswith("CHEAT_") for x in self.report.errors):
            self.ok("cheat PASS")


class CoverageChecker(Checker):
    def run(self):
        report_dir = self.report.report_dir
        report_dir.mkdir(parents=True, exist_ok=True)

        project = TEST / "TestProject.vbproj"
        dotnet = shutil.which("dotnet")
        coverage = shutil.which("dotnet-coverage")
        coverage = Path(coverage) if coverage else Path.home() / ".dotnet" / "tools" / "dotnet-coverage.exe"

        if not project.is_file():
            self.error("COVERAGE_PROJECT", "TestProject.vbproj missing", project)
            return
        if not dotnet:
            self.error("COVERAGE_TOOL", "dotnet SDK not found")
            return
        if not coverage.is_file():
            self.error("COVERAGE_TOOL", "dotnet-coverage not found")
            return

        xml = report_dir / "coverage.cobertura.xml"
        xml.unlink(missing_ok=True)

        restore_cmd = [
            dotnet, "restore", str(project), "--nologo",
        ]
        if not self._run(restore_cmd, ROOT, "restore.log", "COVERAGE_RESTORE", "Restore failed"):
            return

        build_cmd = [
            dotnet, "build", str(project), "--no-restore",
            "--configuration", "Debug", "--nologo",
        ]
        if not self._run(build_cmd, ROOT, "build.log", "COVERAGE_BUILD", "Build failed"):
            return

        target = (
            f'cmd /d /s /c ""{dotnet}" test "{project}" '
            '--no-restore --no-build --configuration Debug '
            f'--results-directory "{report_dir}" '
            '--logger "trx;LogFileName=test-results.trx" '
            '--logger "console;verbosity=normal""'
        )

        p = subprocess.run(
            [str(coverage), "collect", target, "-f", "cobertura", "-o", str(xml)],
            cwd=TEST,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=False,
        )

        test_output = (
            "COMMAND: "
            + str(coverage)
            + " collect "
            + target
            + " -f cobertura -o "
            + str(xml)
            + "\n\n"
            + decode(p.stdout)
        )
        self.report.write_report("test.log", test_output)

        if (
            p.returncode == 3
            or "Total tests: 0" in test_output
            or "未提供任何測試" in test_output
            or "No test is available" in test_output
            or "No tests are available" in test_output
        ):
            self.error(
                "COVERAGE_NO_TESTS",
                "Test runner discovered/executed zero tests",
                fix="Fix the MSTest test class/method attributes before checking coverage.",
                report_name="test.log",
                report_content=test_output,
            )
            return

        if p.returncode != 0 or not xml.is_file():
            self.error(
                "COVERAGE_TEST",
                "Test runner / dotnet-coverage failed",
                f"exit={p.returncode}",
                fix="Open test.log and fix the failing test/coverage command.",
                report_name="test.log",
                report_content=test_output,
            )
            return

        items = parse_coverage(xml)
        threshold = float(self.cfg.get("min_coverage", 90))
        coverage_lines = []

        for target in [self.cfg["entry_function"]] + self.cfg.get("critical_functions", []):
            matches = resolve(items, target)
            if len(matches) != 1:
                nearby = candidates(items, target) or ["no candidates"]
                coverage_lines.append(
                    f"FAIL {target}: mapping failed; candidates={nearby}"
                )
                self.error(
                    "COVERAGE_MAP",
                    "Coverage function mapping failed",
                    target,
                    *nearby,
                    fix="Update function_mapping.json only if the production symbol name is actually different.",
                )
                continue

            pct = float(matches[0].coverage_percent)
            coverage_lines.append(
                f"{'PASS' if pct > threshold else 'FAIL'} {target}: "
                f"{pct:.2f}% (required > {threshold:.2f}%)"
            )

            if pct <= threshold:
                self.error(
                    "COVERAGE_LOW",
                    f"{target} coverage too low",
                    f"{pct:.2f}% <= {threshold:.2f}%",
                    fix="Add/repair E2E cases through Main; do not call mapped functions directly.",
                )
            else:
                self.ok(f"{target}: {pct:.2f}%")

        self.report.write_report(
            "coverage.txt",
            coverage_lines or ["No mapped functions checked."],
        )

        if not any(x.code.startswith("COVERAGE_") for x in self.report.errors):
            self.ok("coverage PASS")

    def _run(self, cmd, cwd, report_name, code, title):
        p = subprocess.run(
            cmd,
            cwd=cwd,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=False,
        )
        output = "COMMAND: " + " ".join(cmd) + "\n\n" + decode(p.stdout)

        if p.returncode:
            self.error(
                code,
                title,
                f"exit={p.returncode}",
                fix="Open the full report and fix the command failure.",
                report_name=report_name,
                report_content=output,
            )
            return False

        self.report.write_report(report_name, output)
        return True


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--block", required=True)
    ap.add_argument("--project-root", default="")
    ap.add_argument("--state-file", default="")
    args = ap.parse_args()

    configure_paths(args.project_root)
    if args.block not in MAPPING:
        ap.error(
            "--block must be one of: "
            + ", ".join(sorted(MAPPING))
        )

    report = ValidatorReport(TEST, f"e2e-{args.block}", stdout_items=20)
    info = []

    StructureChecker(args.block, report, info).run()
    if not report.errors:
        CheatChecker(args.block, report, info).run()
    if not report.errors:
        CoverageChecker(args.block, report, info).run()

    report.write_report("info.txt", info or ["No info."])
    write_review_markdown(args.block, report)
    update_project_review()
    report.write_report(
        "validation_report.txt",
        [
            f"block: {args.block}",
            *info,
            f"errors: {len(report.errors)}",
            f"warnings: {len(report.warnings)}",
            f"status: {report.status()}",
        ],
    )

    if info:
        print("INFO:")
        for line in info:
            print("-", line)

    return report.finish()


if __name__ == "__main__":
    raise SystemExit(main())
