from __future__ import annotations

import re
import xml.etree.ElementTree as ET
from dataclasses import dataclass
from pathlib import Path


@dataclass(frozen=True)
class FunctionCoverage:
    class_name: str
    function_name: str
    signature: str
    covered_lines: int
    total_lines: int
    coverage_percent: float

    @property
    def full_name(self) -> str:
        return f"{self.class_name}.{self.function_name}{self.signature or ''}"


def parse_coverage(path: str | Path):
    root = ET.parse(path).getroot()
    items = []

    for cls in root.findall(".//class"):
        class_name = cls.get("name", "")
        methods = cls.find("methods")
        if methods is None:
            continue

        for method in methods.findall("method"):
            lines_node = method.find("lines")
            lines = [] if lines_node is None else lines_node.findall("line")
            total = len(lines)
            covered = sum(
                1 for line in lines
                if int(line.get("hits", "0") or 0) > 0
            )

            items.append(FunctionCoverage(
                class_name=class_name,
                function_name=method.get("name", ""),
                signature=method.get("signature", ""),
                covered_lines=covered,
                total_lines=total,
                coverage_percent=(covered / total * 100.0 if total else 0.0),
            ))

    return items


def _clean(value: str) -> str:
    value = (value or "").strip().lower()
    value = value.replace("global.", "")
    value = value.replace("global::", "")
    value = value.replace("+", ".")
    value = re.sub(r"`\d+", "", value)
    value = re.sub(r"\s+", "", value)
    return value


def _strip_signature(value: str) -> str:
    value = _clean(value)
    return value.split("(", 1)[0]


def _simple_class_name(value: str) -> str:
    value = _clean(value)
    return value.rsplit(".", 1)[-1]


def resolve(items, query):
    """
    Resolve Class.Method regardless of common Cobertura naming differences.

    Supported examples:
      SampleBlockEngine.Execute
      Namespace.SampleBlockEngine.Execute
      SampleBlockEngine.Execute(System.Int32)
      Namespace.SampleBlockEngine.Execute(System.Int32)
    """
    query_base = _strip_signature(query)

    if "." in query_base:
        query_class, query_method = query_base.rsplit(".", 1)
    else:
        query_class, query_method = "", query_base

    query_class_simple = _simple_class_name(query_class)

    matches = []

    for item in items:
        method = _clean(item.function_name)
        class_full = _clean(item.class_name)
        class_simple = _simple_class_name(class_full)

        # Some Cobertura producers include Class.Method in method@name.
        method_base = _strip_signature(method)
        if "." in method_base:
            method_owner, method_only = method_base.rsplit(".", 1)
        else:
            method_owner, method_only = "", method_base

        if method_only != query_method:
            continue

        if not query_class:
            matches.append(item)
            continue

        class_match = (
            class_full == query_class
            or class_full.endswith("." + query_class)
            or class_simple == query_class_simple
            or method_owner == query_class
            or method_owner.endswith("." + query_class)
            or _simple_class_name(method_owner) == query_class_simple
        )

        if class_match:
            matches.append(item)

    return matches


def candidates(items, query, limit=10):
    """Return useful same-method candidates for validator diagnostics."""
    query_base = _strip_signature(query)
    query_method = query_base.rsplit(".", 1)[-1]

    same_method = [
        item.full_name
        for item in items
        if _strip_signature(item.function_name).rsplit(".", 1)[-1] == query_method
    ]

    if same_method:
        return same_method[:limit]

    return [item.full_name for item in items[:limit]]
