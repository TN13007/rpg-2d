#!/usr/bin/env python3
"""Validate Godot 4 text scene structure before the engine import step.

This is a focused structural checker, not a replacement for Godot's parser.
The headless editor remains authoritative for parsing and resource loading.
"""
from __future__ import annotations

import argparse
import re
import sys
from dataclasses import dataclass
from pathlib import Path

SECTION_ORDER = {
    "gd_scene": -1,
    "ext_resource": 0,
    "sub_resource": 1,
    "node": 2,
    "connection": 3,
}
HEADER_PATTERN = re.compile(r'^\[(gd_scene|ext_resource|sub_resource|node|connection)(?:\s+(.*?))?\]\s*(?:;.*)?$')
QUOTED_ATTRIBUTE = re.compile(r'\b([A-Za-z_][A-Za-z0-9_]*)\s*=\s*"((?:\\.|[^"])*)"')
RESOURCE_REFERENCE = re.compile(r'\b(ExtResource|SubResource)\s*\(\s*"([^"]+)"\s*\)')
RESOURCE_PATH = re.compile(r'"(res://[^"]+)"')


@dataclass(frozen=True)
class SceneIssue:
    path: Path
    line: int
    message: str

    def __str__(self) -> str:
        return f"{self.path}:{self.line}: error: {self.message}"


def _attributes(raw: str) -> dict[str, str]:
    return {key: value for key, value in QUOTED_ATTRIBUTE.findall(raw)}


def _quoted_value(raw: str, key: str) -> str | None:
    match = re.search(rf'\b{re.escape(key)}\s*=\s*"((?:\\.|[^"])*)"', raw)
    return match.group(1) if match else None


def validate_scene(scene_path: Path, project_root: Path) -> list[SceneIssue]:
    issues: list[SceneIssue] = []
    try:
        text = scene_path.read_text(encoding="utf-8")
    except (OSError, UnicodeError) as error:
        return [SceneIssue(scene_path, 1, f"cannot read scene: {error}")]

    entries: list[tuple[int, str, str]] = []
    current_section: str | None = None
    descriptor_count = 0
    last_rank = -1

    for line_number, raw_line in enumerate(text.splitlines(), start=1):
        stripped = raw_line.strip()
        if not stripped or stripped.startswith(";"):
            continue

        if stripped.startswith("["):
            match = HEADER_PATTERN.fullmatch(stripped)
            if not match:
                issues.append(SceneIssue(scene_path, line_number, "malformed or unsupported section header"))
                current_section = None
                continue

            section, attributes = match.groups()
            if section == "gd_scene":
                descriptor_count += 1
                if line_number != next(
                    (number for number, candidate in enumerate(text.splitlines(), start=1)
                     if candidate.strip() and not candidate.strip().startswith(";")),
                    line_number,
                ):
                    issues.append(SceneIssue(scene_path, line_number, "scene descriptor must be the first non-comment entry"))
                if descriptor_count > 1:
                    issues.append(SceneIssue(scene_path, line_number, "scene must contain exactly one [gd_scene] descriptor"))
                format_match = re.search(r'\bformat\s*=\s*(\d+)', attributes)
                if not format_match or format_match.group(1) != "3":
                    issues.append(SceneIssue(scene_path, line_number, "Godot 4 scenes must declare format=3"))
            else:
                rank = SECTION_ORDER[section]
                if rank < last_rank:
                    issues.append(SceneIssue(
                        scene_path,
                        line_number,
                        f"[{section}] section is out of order; TSCN sections must be ext_resource, sub_resource, node, connection",
                    ))
                last_rank = max(last_rank, rank)
                entries.append((line_number, section, attributes))
            current_section = section
            continue

        if current_section is None:
            issues.append(SceneIssue(scene_path, line_number, "content appears outside a valid TSCN section"))

    if descriptor_count == 0:
        issues.append(SceneIssue(scene_path, 1, "missing [gd_scene] descriptor"))

    declared_resources: dict[str, set[str]] = {"ext_resource": set(), "sub_resource": set()}
    node_entries: list[tuple[int, dict[str, str]]] = []

    for line_number, section, raw_attributes in entries:
        attributes = _attributes(raw_attributes)
        if section in declared_resources:
            resource_id = _quoted_value(raw_attributes, "id")
            if not resource_id:
                issues.append(SceneIssue(scene_path, line_number, f"[{section}] is missing a quoted id"))
            elif resource_id in declared_resources[section]:
                issues.append(SceneIssue(scene_path, line_number, f"duplicate {section} id {resource_id!r}"))
            else:
                declared_resources[section].add(resource_id)

            if section == "ext_resource":
                resource_path = _quoted_value(raw_attributes, "path")
                if resource_path:
                    _validate_resource_path(resource_path, scene_path, project_root, line_number, issues)

        elif section == "node":
            name = _quoted_value(raw_attributes, "name")
            node_type = _quoted_value(raw_attributes, "type")
            parent = _quoted_value(raw_attributes, "parent")
            if not name:
                issues.append(SceneIssue(scene_path, line_number, "[node] is missing a quoted name"))
            if not node_type and "instance=" not in raw_attributes:
                issues.append(SceneIssue(scene_path, line_number, "[node] must declare type or instance"))
            if name:
                node_entries.append((line_number, {
                    "name": name,
                    "parent": parent if parent is not None else "",
                }))

    root_nodes = [(line, node) for line, node in node_entries if not node["parent"]]
    if len(root_nodes) != 1:
        issues.append(SceneIssue(
            scene_path,
            root_nodes[1][0] if len(root_nodes) > 1 else 1,
            f"scene must have exactly one root node; found {len(root_nodes)}",
        ))

    root_name = root_nodes[0][1]["name"] if len(root_nodes) == 1 else ""
    known_paths: set[str] = {"."}
    node_paths: list[tuple[int, str, str]] = []

    for line_number, node in node_entries:
        name = node["name"]
        parent = node["parent"]
        if not parent:
            node_paths.append((line_number, ".", name))
            continue
        if parent != "." and root_name and (parent == root_name or parent.startswith(root_name + "/")):
            issues.append(SceneIssue(scene_path, line_number, f"node parent {parent!r} must not include the scene root name"))
        node_path = name if parent == "." else f"{parent}/{name}"
        node_paths.append((line_number, parent, node_path))

    for line_number, parent, node_path in node_paths:
        if parent != "." and parent not in known_paths:
            # Defer resolution until all paths have been collected.
            pass
        if node_path in known_paths:
            issues.append(SceneIssue(scene_path, line_number, f"duplicate node path {node_path!r}"))
        known_paths.add(node_path)

    all_node_paths = {node_path for _, _, node_path in node_paths}
    for line_number, parent, _ in node_paths:
        if parent and parent != "." and parent not in all_node_paths:
            issues.append(SceneIssue(scene_path, line_number, f"node parent path {parent!r} does not exist"))

    for line_number, raw_line in enumerate(text.splitlines(), start=1):
        code = raw_line.split(";", 1)[0]
        for reference in RESOURCE_REFERENCE.finditer(code):
            kind, resource_id = reference.groups()
            section = "ext_resource" if kind == "ExtResource" else "sub_resource"
            if resource_id not in declared_resources[section]:
                issues.append(SceneIssue(scene_path, line_number, f"{kind} references undeclared id {resource_id!r}"))
        for resource_match in RESOURCE_PATH.finditer(code):
            _validate_resource_path(resource_match.group(1), scene_path, project_root, line_number, issues)

    return issues


def _validate_resource_path(
    resource_path: str,
    scene_path: Path,
    project_root: Path,
    line_number: int,
    issues: list[SceneIssue],
) -> None:
    if not resource_path.startswith("res://"):
        return
    target = project_root / resource_path.removeprefix("res://")
    if not target.exists():
        issues.append(SceneIssue(scene_path, line_number, f"resource path does not exist: {resource_path}"))


def find_scenes(project_root: Path, paths: list[str]) -> list[Path]:
    if paths:
        return [((project_root / path).resolve() if not Path(path).is_absolute() else Path(path)) for path in paths]
    ignored = {".git", ".godot", "bin", "obj"}
    return sorted(
        path for path in project_root.rglob("*.tscn")
        if not any(part in ignored for part in path.relative_to(project_root).parts)
    )


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("paths", nargs="*", help="Optional scene paths, relative to the project root")
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()

    project_root = args.project_root.resolve()
    scenes = find_scenes(project_root, args.paths)
    if not scenes:
        print("No .tscn files found.")
        return 0

    issues = [issue for scene in scenes for issue in validate_scene(scene, project_root)]
    if issues:
        for issue in issues:
            print(issue, file=sys.stderr)
        print(f"Scene validation failed: {len(issues)} issue(s) across {len(scenes)} scene(s).", file=sys.stderr)
        return 1

    print(f"Scene structure valid: {len(scenes)} scene(s) checked.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
