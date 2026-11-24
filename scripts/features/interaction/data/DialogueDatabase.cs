using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace RPG2D.Features.Interaction.Data
{
    public sealed class DialogueDatabase
    {
        private const string BuiltInManifestPath = "res://data/dialogues/dialogues.json";
        private const string ModDirectoryPath = "user://mods/dialogues";

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly Dictionary<string, DialogueDefinition> _dialogues =
            new(StringComparer.OrdinalIgnoreCase);

        public static DialogueDatabase Load()
        {
            DialogueDatabase database = new();

            database.LoadManifest(_builtInManifestPath, isMod: false);
            database.LoadMods();

            GD.Print($"Dialogue database loaded: {database._dialogues.Count} dialogue(s).");
            return database;
        }

        public bool TryGetDialogue(string id, out DialogueDefinition dialogue)
        {
            return _dialogues.TryGetValue(id, out dialogue!);
        }

        private void LoadMods()
        {
            Error directoryError = DirAccess.MakeDirRecursiveAbsolute(
                ProjectSettings.GlobalizePath(_modDirectoryPath)
            );

            if (directoryError != Error.Ok && directoryError != Error.AlreadyExists)
            {
                GD.PushWarning($"Could not create dialogue mod directory: {directoryError}");
                return;
            }

            string[] modFiles = DirAccess.GetFilesAt(_modDirectoryPath)
                .Where(file => file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (string fileName in modFiles)
            {
                LoadManifest($"{_modDirectoryPath}/{fileName}", isMod: true);
            }
        }

        private void LoadManifest(string path, bool isMod)
        {
            Godot.FileAccess? file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PushWarning($"Could not read dialogue file '{path}': {Godot.FileAccess.GetOpenError()}");
                return;
            }

            string json = file.GetAsText();
            file.Close();
            DialogueManifest? manifest;

            try
            {
                manifest = JsonSerializer.Deserialize<DialogueManifest>(json, _jsonOptions);
            }
            catch (JsonException exception)
            {
                GD.PushWarning($"Invalid dialogue JSON in '{path}': {exception.Message}");
                return;
            }

            if (manifest == null || manifest.SchemaVersion != 1 || manifest.Dialogues == null)
            {
                GD.PushWarning(
                    $"Unsupported or invalid dialogue manifest '{path}'. Expected schema_version 1 and a dialogues array."
                );
                return;
            }

            foreach (DialogueDefinition? dialogue in manifest.Dialogues)
            {
                if (!IsValid(dialogue, path))
                {
                    continue;
                }

                if (_dialogues.ContainsKey(dialogue!.Id) && !isMod)
                {
                    GD.PushWarning($"Duplicate built-in dialogue ID '{dialogue.Id}' in '{path}'.");
                    continue;
                }

                if (_dialogues.ContainsKey(dialogue!.Id))
                {
                    GD.Print($"Dialogue '{dialogue.Id}' overridden by '{path}'.");
                }

                _dialogues[dialogue.Id] = dialogue;
            }
        }

        private static bool IsValid(DialogueDefinition? dialogue, string path)
        {
            if (dialogue == null ||
                string.IsNullOrWhiteSpace(dialogue.Id) ||
                string.IsNullOrWhiteSpace(dialogue.Title) ||
                dialogue.Message == null ||
                dialogue.Choices == null)
            {
                GD.PushWarning($"Skipping invalid dialogue entry in '{path}'.");
                return false;
            }

            HashSet<string> choiceIds = new(StringComparer.OrdinalIgnoreCase);

            foreach (DialogueChoiceDefinition? choice in dialogue.Choices)
            {
                if (choice == null ||
                    string.IsNullOrWhiteSpace(choice.Id) ||
                    string.IsNullOrWhiteSpace(choice.Text) ||
                    !choiceIds.Add(choice.Id))
                {
                    GD.PushWarning(
                        $"Skipping dialogue '{dialogue.Id}' in '{path}': choices need unique, non-empty IDs and text."
                    );
                    return false;
                }
            }

            return true;
        }

        public sealed class DialogueManifest
        {
            [JsonPropertyName("schemaVersion")]
            public int SchemaVersion { get; set; }

            [JsonPropertyName("dialogues")]
            public List<DialogueDefinition>? Dialogues { get; set; }
        }
    }
}