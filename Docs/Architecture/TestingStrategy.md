# Testing Strategy

Testing verifies observable behavior and protects previously working behavior. A passing build is not proof that a gameplay feature works.

## Test levels

| Level | Purpose | Location |
|---|---|---|
| Unit | Deterministic rules/state transformations without scene rendering | `Tests/` |
| Integration | Collaborating components, content loading and scene/service boundaries | `Tests/` |
| Tooling | Repository validators, naming and documentation checks | `Tools/Tests/` |
| Runtime smoke | Project import, main scene load and critical transitions | Godot headless/editor |
| Manual playtest | Input feel, collision, readability, pacing, UI and player journey | Godot editor/game |

Prefer unit tests for pure rules. Use integration tests when behavior depends on multiple real components. Assert public behavior rather than private implementation details.

## Test naming and structure
- .NET test project: `Tests/RPG2D.Tests.csproj`.
- C# test files/classes use PascalCase, e.g. `PlayerMovementTests.cs`.
- Test methods use descriptive PascalCase: `Move_WhenBlocked_DoesNotChangePosition`.
- Group tests by behavior/feature; avoid one giant test class.
- Python repository-tool tests live in `Tools/Tests/` and use `Test*.py`.
- Keep fixtures minimal, deterministic and independent between tests.

## Coverage expectations

For each behavior change, consider:
1. Normal successful behavior.
2. Invalid or unavailable input.
3. Boundaries: zero, maximum, empty, missing reference and exact thresholds.
4. State invariants and resource accounting.
5. Regression cases for the reported bug or previously broken flow.

A test must fail when the intended behavior is broken and pass when it is correct. Do not add tests merely to increase test count.

## Godot verification

Automated tests cannot fully validate scene wiring, exported properties, node paths, input maps, animation timing, collision placement, visual hierarchy or player comprehension.

When a change affects scenes, nodes, resources, signals, input, collision, animation, UI or transitions:
- Open the project in Godot 4.7.2 .NET.
- Confirm resource import completes without errors.
- Open/run the main scene and each affected scene.
- Exercise the changed journey using actual input.
- Check Output and Debugger for errors and warnings.
- Record what was manually exercised and what remains untested.

Use headless import/build/smoke checks where supported, but do not claim they replace manual playtesting.

## Local and CI validation

Run from the repository root:

```bash
python Tools/Validate.py --base origin/main --head HEAD
```

For a PR branch, add `--branch <branch-name> --check-branch`. The unified script runs Python validator tests, Markdown link/anchor validation, changed-path/JSON naming validation and .NET Release tests. It stops at the first failed command and propagates the non-zero exit code. GitHub Actions calls the same entry point.

## Test quality
- Tests must be deterministic and independent of wall-clock time, network and machine-specific paths unless explicitly testing that dependency.
- Avoid arbitrary sleeps; use controlled inputs or synchronization.
- Do not swallow exceptions in test helpers.
- Regression tests assert the externally observable symptom.
- Keep test data small and representative.
- Update tests when behavior contracts change; do not weaken assertions just to make CI green.
- If a behavior cannot be automated reliably, state why and provide a reproducible manual procedure in the PR.
