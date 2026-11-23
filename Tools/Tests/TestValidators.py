"""Regression tests for repository validation rules."""
import importlib.util
import json
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def load(name, path):
    spec = importlib.util.spec_from_file_location(name, ROOT / path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


naming = load("naming", "Tools/CheckProjectNaming.py")
markdown = load("markdown", "Tools/CheckMarkdown.py")


class NamingTests(unittest.TestCase):
    def test_branch_accepts_issue_scoped_name(self):
        self.assertIsNotNone(naming.BRANCH.fullmatch("feat/issue-12-room-exploration"))

    def test_branch_rejects_missing_issue(self):
        self.assertIsNone(naming.BRANCH.fullmatch("feat/room-exploration"))

    def test_snake_case_game_paths_pass(self):
        self.assertEqual([], naming.validate_path("scenes/exploration/player_controller.tscn"))
        self.assertEqual([], naming.validate_path("scripts/features/exploration/PlayerController.cs"))
        self.assertEqual([], naming.validate_path("scenes/exploration/dormitory_room.tscn"))

    def test_pascal_case_csharp_files_pass(self):
        self.assertEqual([], naming.validate_path("scripts/features/exploration/PlayerController.cs"))
        self.assertEqual([], naming.validate_path("Tests/PlayerMovementTests.cs"))

    def test_invalid_game_paths_fail(self):
        self.assertTrue(naming.validate_path("Scripts/features/player_controller.cs"))
        self.assertTrue(naming.validate_path("scenes/Exploration/DormitoryRoom.tscn"))

    def test_infrastructure_names_are_preserved(self):
        self.assertEqual([], naming.validate_path("Docs/Architecture/CodingStandards.md"))
        self.assertEqual([], naming.validate_path("Tools/CheckProjectNaming.py"))

    def test_json_property_and_id_conventions(self):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "content.json"
            path.write_text(json.dumps({
                "characterId": "player_ari",
                "displayName": "Ari",
                "invalid_property": "x",
            }), encoding="utf-8")
            issues = naming.validate_json(str(path))
        self.assertTrue(any("invalid_property" in item for item in issues))
        self.assertFalse(any("player_ari" in item for item in issues))

    def test_invalid_id_fails(self):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "content.json"
            path.write_text(json.dumps({"id": "PlayerAri"}), encoding="utf-8")
            issues = naming.validate_json(str(path))
        self.assertTrue(any("snake_case" in item for item in issues))


class MarkdownTests(unittest.TestCase):
    def test_heading_anchor(self):
        self.assertIn("project-structure", markdown.anchors("# Project Structure"))

    def test_duplicate_headings_are_numbered(self):
        self.assertEqual({"setup", "setup-1"}, markdown.anchors("# Setup\n\n# Setup"))


if __name__ == "__main__":
    unittest.main()
