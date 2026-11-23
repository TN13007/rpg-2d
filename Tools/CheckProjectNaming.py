#!/usr/bin/env python3
"""Validate changed paths and authored JSON naming conventions."""
from __future__ import annotations
import argparse
import json
import re
import subprocess
import sys
from pathlib import PurePosixPath

PASCAL = re.compile(r"^[A-Z][A-Za-z0-9]*$")
CAMEL = re.compile(r"^[a-z][A-Za-z0-9]*$")
SNAKE = re.compile(r"^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$")
BRANCH = re.compile(r"^(feat|fix|docs|test|refactor|chore)/issue-[1-9][0-9]*-[a-z0-9]+(?:-[a-z0-9]+)*$")
ROOT_FILES = {"README.md", "CONTRIBUTING.md", "AGENTS.md", "WORKFLOW.md", "project.godot", "icon.svg", "icon.svg.import", ".gitignore", ".editorconfig", "RPG2D.csproj"}
INFRA_DIRS = {"Docs", "Tools", "Tests", ".github", "Ninja Adventure - Asset Pack"}
ENUM_FIELDS = {"type", "effectType", "targetType", "timeMode", "stat"}


def git(*args):
    return subprocess.run(["git", *args], text=True, capture_output=True, check=True).stdout


def changed_paths(base, head):
    parts = git("diff", "--name-status", "-z", f"{base}...{head}").split("\0")
    result = []
    index = 0
    while index < len(parts) and parts[index]:
        status = parts[index]
        index += 1
        if status.startswith(("R", "C")):
            index += 1
            path = parts[index]
            index += 1
        else:
            path = parts[index]
            index += 1
        if status[0] != "D":
            result.append((status[0], path))
    return result


def validate_path(path):
    issues = []
    parts = PurePosixPath(path).parts
    if any(part in INFRA_DIRS for part in parts):
        # Infrastructure and vendor paths keep their established naming.
        # C# source/project files still follow their language conventions.
        name = parts[-1]
        if name.endswith(".cs") and not PASCAL.fullmatch(name[:-3]):
            issues.append(f"{path}: C# file name must match its PascalCase class name.")
        if name.endswith(".csproj") and not all(PASCAL.fullmatch(part) for part in name[:-7].split(".")):
            issues.append(f"{path}: C# project file stem must use PascalCase segments.")
        return issues

    for directory in parts[:-1]:
        if directory.startswith("."):
            continue
        if not SNAKE.fullmatch(directory):
            issues.append(f"{path}: game-owned directory '{directory}' must use snake_case.")

    name = parts[-1]
    if name in ROOT_FILES or name in {".gitkeep", "LICENSE", "LICENSE.txt", "LICENSE.md"}:
        return issues
    if name.startswith(".") or name.endswith((".import", ".uid")):
        return issues

    suffix = PurePosixPath(name).suffix
    stem = name[:-len(suffix)] if suffix else name
    if suffix in {".cs", ".csproj"}:
        valid = bool(PASCAL.fullmatch(stem))
        convention = "PascalCase"
    else:
        valid = bool(SNAKE.fullmatch(stem))
        convention = "snake_case"
    if not valid:
        issues.append(f"{path}: game-owned file stem '{stem}' must use {convention}.")
    return issues


def validate_json(path):
    try:
        with open(path, encoding="utf-8") as source:
            data = json.load(source)
    except (OSError, json.JSONDecodeError) as error:
        return [f"{path}: invalid or unreadable JSON: {error}"]
    issues = []

    def visit(value, location="$"):
        if isinstance(value, dict):
            for key, child in value.items():
                if not CAMEL.fullmatch(key):
                    issues.append(f"{path}:{location}: property '{key}' must use camelCase.")
                if key == "id" or key.endswith(("Id", "Ids")):
                    values = child if isinstance(child, list) else [child]
                    for identifier in values:
                        if isinstance(identifier, str) and not SNAKE.fullmatch(identifier):
                            issues.append(f"{path}:{location}.{key}: ID '{identifier}' must use snake_case.")
                if key in ENUM_FIELDS and isinstance(child, str) and not PASCAL.fullmatch(child):
                    issues.append(f"{path}:{location}.{key}: enum value '{child}' must use PascalCase.")
                visit(child, f"{location}.{key}")
        elif isinstance(value, list):
            for index, child in enumerate(value):
                visit(child, f"{location}[{index}]")

    visit(data)
    return issues


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--base", required=True)
    parser.add_argument("--head", default="HEAD")
    parser.add_argument("--branch", default="")
    parser.add_argument("--check-branch", action="store_true")
    args = parser.parse_args()
    issues = []
    try:
        changes = changed_paths(args.base, args.head)
    except (subprocess.CalledProcessError, OSError) as error:
        print(f"Could not inspect Git changes: {error}", file=sys.stderr)
        return 2
    if args.check_branch and not BRANCH.fullmatch(args.branch):
        issues.append(f"Branch '{args.branch}' must match '<type>/issue-<number>-<kebab-case-description>'.")
    for _, path in changes:
        issues.extend(validate_path(path))
        if path.lower().endswith(".json"):
            issues.extend(validate_json(path))
    if issues:
        print("Naming validation failed:")
        for issue in issues:
            print(f"- {issue}")
        return 1
    print(f"Naming validation passed ({len(changes)} changed paths).")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
