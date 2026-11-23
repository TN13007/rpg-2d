#!/usr/bin/env python3
"""Validate tracked Markdown links and heading fragments."""
from __future__ import annotations
import re
import subprocess
import sys
from pathlib import Path
from urllib.parse import unquote, urlsplit

ROOT = Path(__file__).resolve().parents[1]
LINK = re.compile(r"!?(?:\[[^\]]*\])\((<[^>]+>|[^)\s]+)(?:\s+[^)]*)?\)")
HEADING = re.compile(r"^#{1,6}\s+(.+?)\s*#*\s*$")
FENCE = re.compile(r"^\s*(" + re.escape(chr(96)) + r"{3,}|~{3,})")

def strip_fences(text):
    output = []
    marker = ""
    length = 0
    for line in text.splitlines():
        match = FENCE.match(line)
        if match:
            token = match.group(1)
            if not marker:
                marker, length = token[0], len(token)
            elif token[0] == marker and len(token) >= length:
                marker, length = "", 0
            continue
        if not marker:
            output.append(line)
    return output

def slug(text):
    tick = chr(96)
    text = re.sub(tick + r"([^" + tick + r"]+)" + tick, r"\1", text)
    text = re.sub(r"!?\[([^\]]+)\]\([^)]*\)", r"\1", text)
    text = text.replace("*", "").lower()
    text = re.sub(r"[^\w\- ]", "", text)
    return re.sub(r"\s+", "-", text).strip("-")

def anchors(text):
    result = set()
    counts = {}
    for line in strip_fences(text):
        match = HEADING.match(line)
        if not match:
            continue
        base = slug(match.group(1))
        count = counts.get(base, 0)
        counts[base] = count + 1
        result.add(base if count == 0 else f"{base}-{count}")
    return result

def validate(root=ROOT):
    root = root.resolve()
    tracked = subprocess.run(["git", "ls-files"], cwd=root, text=True, capture_output=True, check=True).stdout.splitlines()
    files = set(tracked)
    issues = []
    for relative in sorted(path for path in files if path.lower().endswith(".md")):
        source = root / relative
        if not source.is_file():
            continue
        for number, line in enumerate(strip_fences(source.read_text(encoding="utf-8")), 1):
            tick = chr(96)
            line = re.sub(r"(?<!" + tick + ")" + tick + r"+[^" + tick + r"]*" + tick + r"+", "", line)
            for match in LINK.finditer(line):
                target = match.group(1).strip("<>")
                parsed = urlsplit(target)
                if parsed.scheme or parsed.netloc:
                    continue
                raw_path = unquote(parsed.path)
                destination = (source.parent / raw_path).resolve() if raw_path else source
                try:
                    relative_target = destination.relative_to(root).as_posix()
                except ValueError:
                    issues.append(f"{relative}:{number}: link escapes repository: {target}")
                    continue
                if relative_target not in files:
                    issues.append(f"{relative}:{number}: missing tracked target: {target}")
                    continue
                if parsed.fragment and destination.suffix.lower() == ".md":
                    if unquote(parsed.fragment) not in anchors(destination.read_text(encoding="utf-8")):
                        issues.append(f"{relative}:{number}: missing heading anchor: {target}")
    return issues

def main():
    try:
        issues = validate()
    except (OSError, subprocess.CalledProcessError) as error:
        print(f"Markdown validation could not run: {error}", file=sys.stderr)
        return 2
    if issues:
        print("Markdown validation failed:")
        for issue in issues:
            print(f"- {issue}")
        return 1
    print("Markdown links and anchors passed.")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
