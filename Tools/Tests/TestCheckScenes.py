from __future__ import annotations

import tempfile
import unittest
from pathlib import Path

from Tools.CheckScenes import validate_scene


class CheckScenesTests(unittest.TestCase):
    def setUp(self) -> None:
        self.temporary_directory = tempfile.TemporaryDirectory()
        self.project_root = Path(self.temporary_directory.name)
        self.scene_path = self.project_root / "scenes" / "sample.tscn"
        self.scene_path.parent.mkdir(parents=True)

    def tearDown(self) -> None:
        self.temporary_directory.cleanup()

    def write_scene(self, content: str) -> list:
        self.scene_path.write_text(content, encoding="utf-8")
        return validate_scene(self.scene_path, self.project_root)

    def test_valid_scene_with_sub_resource_before_nodes_passes(self) -> None:
        issues = self.write_scene(
            '[gd_scene format=3]\n'
            '[sub_resource type="RectangleShape2D" id="Shape"]\n'
            'size = Vector2(10, 10)\n'
            '[node name="Root" type="Node2D"]\n'
            '[node name="Child" type="CollisionShape2D" parent="."]\n'
            'shape = SubResource("Shape")\n'
        )
        self.assertEqual([], issues)

    def test_sub_resource_after_node_is_rejected(self) -> None:
        issues = self.write_scene(
            '[gd_scene format=3]\n'
            '[node name="Root" type="Node2D"]\n'
            '[sub_resource type="RectangleShape2D" id="Shape"]\n'
        )
        self.assertTrue(any("out of order" in issue.message for issue in issues))

    def test_scene_requires_exactly_one_root(self) -> None:
        issues = self.write_scene(
            '[gd_scene format=3]\n'
            '[node name="First" type="Node2D"]\n'
            '[node name="Second" type="Node2D"]\n'
        )
        self.assertTrue(any("exactly one root" in issue.message for issue in issues))

    def test_missing_parent_path_is_rejected(self) -> None:
        issues = self.write_scene(
            '[gd_scene format=3]\n'
            '[node name="Root" type="Node2D"]\n'
            '[node name="Child" type="Node2D" parent="Missing"]\n'
        )
        self.assertTrue(any("parent path" in issue.message for issue in issues))

    def test_undeclared_sub_resource_reference_is_rejected(self) -> None:
        issues = self.write_scene(
            '[gd_scene format=3]\n'
            '[node name="Root" type="Node2D"]\n'
            'material = SubResource("Missing")\n'
        )
        self.assertTrue(any("undeclared id" in issue.message for issue in issues))

    def test_duplicate_sub_resource_id_is_rejected(self) -> None:
        issues = self.write_scene(
            '[gd_scene format=3]\n'
            '[sub_resource type="RectangleShape2D" id="Shape"]\n'
            '[sub_resource type="CircleShape2D" id="Shape"]\n'
            '[node name="Root" type="Node2D"]\n'
        )
        self.assertTrue(any("duplicate sub_resource id" in issue.message for issue in issues))

    def test_missing_res_path_is_rejected(self) -> None:
        issues = self.write_scene(
            '[gd_scene format=3]\n'
            '[ext_resource type="Texture2D" path="res://missing.png" id="Texture"]\n'
            '[node name="Root" type="Node2D"]\n'
        )
        self.assertTrue(any("resource path does not exist" in issue.message for issue in issues))

    def test_external_and_internal_resource_ids_are_separate_namespaces(self) -> None:
        texture = self.project_root / "texture.png"
        texture.write_bytes(b"fixture")
        issues = self.write_scene(
            '[gd_scene format=3]\n'
            '[ext_resource type="Texture2D" path="res://texture.png" id="Shared"]\n'
            '[sub_resource type="RectangleShape2D" id="Shared"]\n'
            '[node name="Root" type="Node2D"]\n'
            'texture = ExtResource("Shared")\n'
            'shape = SubResource("Shared")\n'
        )
        self.assertEqual([], issues)


if __name__ == "__main__":
    unittest.main()
