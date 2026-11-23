#!/usr/bin/env python3
"""Run the same validation checks locally and in GitHub Actions."""
from __future__ import annotations
import argparse
import shlex
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def run(command):
    print(f"\n> {shlex.join(command)}", flush=True)
    result = subprocess.run(command, cwd=ROOT, check=False)
    if result.returncode:
        print(f"Validation stopped: exit code {result.returncode}.", file=sys.stderr)
    return result.returncode

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--base", required=True)
    parser.add_argument("--head", default="HEAD")
    parser.add_argument("--branch", default="")
    parser.add_argument("--check-branch", action="store_true")
    args = parser.parse_args()
    commands = [
        [sys.executable, "-m", "unittest", "discover", "-s", "Tools/Tests", "-p", "Test*.py", "-v"],
        [sys.executable, "Tools/CheckMarkdown.py"],
        [sys.executable, "Tools/CheckProjectNaming.py", "--base", args.base, "--head", args.head, "--branch", args.branch],
    ]
    if args.check_branch:
        commands[-1].append("--check-branch")
    for command in commands:
        code = run(command)
        if code:
            return code
    code = run(["dotnet", "test", "Tests/RPG2D.Tests.csproj", "--configuration", "Release"])
    if code:
        return code
    print("\nAll configured validation checks passed.")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
