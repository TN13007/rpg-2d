using System;
using System.Collections.Generic;
using Godot;
using RPG2D.Features.Interaction;
using RPG2D.Features.Interaction.Data;
using RPG2D.Features.PlayerData;
using RPG2D.Features.Shop;
using PlayerDataState = RPG2D.Features.PlayerData.PlayerData;

namespace RPG2D.Features.Exploration
{
    public partial class PlayerInteractionController : Node
    {
        private PlayerController _playerController = null!;
        private TerminalController? _currentTerminal;
        private TerminalInteractionUI _interactionUi = null!;
        private DialogueDatabase _dialogueDatabase = null!;
        private PlayerDataState _playerData = null!;
        private ShopController _shopController = null!;
        private DialogueDefinition? _currentDialogue;
        private IReadOnlyList<DialogueChoiceDefinition> _choiceOptions =
            Array.Empty<DialogueChoiceDefinition>();

        private int _selectedChoiceIndex;
        private bool _isInteractionOpen;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            _playerController = GetParent<PlayerController>();
            _dialogueDatabase = DialogueDatabase.Load();
            ItemDatabase itemDatabase = ItemDatabase.Load();
            _playerData = PlayerDataDatabase.Load(itemDatabase);

            _interactionUi = new TerminalInteractionUI();
            AddChild(_interactionUi);
            _interactionUi.ChoiceHovered += SelectChoice;
            _interactionUi.ChoiceConfirmed += OnChoiceConfirmed;

            _shopController = new ShopController();
            AddChild(_shopController);
            _shopController.Configure(_playerData, itemDatabase);
            _shopController.LeaveRequested += ReturnToDialogueMenu;
        }

        public override void _Process(double delta)
        {
            if (_isInteractionOpen)
            {
                HandleInteractionInput();
                return;
            }

            if (_currentTerminal != null && Input.IsActionJustPressed("interact"))
            {
                OpenInteraction();
            }
        }

        public void SetCurrentTerminal(TerminalController terminal)
        {
            _currentTerminal = terminal;

            if (!_isInteractionOpen)
            {
                _interactionUi.SetPromptVisible(true);
            }
        }

        public void ClearCurrentTerminal(TerminalController terminal)
        {
            if (_currentTerminal != terminal)
            {
                return;
            }

            _currentTerminal = null;

            if (!_isInteractionOpen)
            {
                _interactionUi.SetPromptVisible(false);
            }
        }

        private void HandleInteractionInput()
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                CloseInteraction();
                return;
            }

            if (_shopController.IsOpen)
            {
                _shopController.HandleInput();
                return;
            }

            if (Input.IsActionJustPressed("interaction_menu_up"))
            {
                MoveSelection(-1);
                return;
            }

            if (Input.IsActionJustPressed("interaction_menu_down"))
            {
                MoveSelection(1);
                return;
            }

            if (Input.IsActionJustPressed("interaction_confirm"))
            {
                ConfirmSelection();
            }
        }

        private void MoveSelection(int direction)
        {
            if (_choiceOptions.Count == 0)
            {
                return;
            }

            _selectedChoiceIndex =
                (_selectedChoiceIndex + direction + _choiceOptions.Count) % _choiceOptions.Count;
            _interactionUi.SetSelectedChoice(_selectedChoiceIndex);
        }

        private void SelectChoice(int index)
        {
            if (!_isInteractionOpen ||
                _shopController.IsOpen ||
                index < 0 ||
                index >= _choiceOptions.Count ||
                _selectedChoiceIndex == index)
            {
                return;
            }

            _selectedChoiceIndex = index;
            _interactionUi.SetSelectedChoice(_selectedChoiceIndex);
        }

        private void OnChoiceConfirmed(int index)
        {
            if (index < 0 || index >= _choiceOptions.Count)
            {
                return;
            }

            SelectChoice(index);
            ConfirmSelection();
        }

        private void ConfirmSelection()
        {
            if (_choiceOptions.Count == 0)
            {
                AdvanceDialogue();
                return;
            }

            if (_selectedChoiceIndex < 0 ||
                _selectedChoiceIndex >= _choiceOptions.Count)
            {
                return;
            }

            DialogueChoiceDefinition choice = _choiceOptions[_selectedChoiceIndex];

            if (string.Equals(choice.Action, "openShop", StringComparison.OrdinalIgnoreCase))
            {
                OpenShop();
                return;
            }

            if (choice.ClosesInteraction)
            {
                CloseInteraction();
                return;
            }

            if (!string.IsNullOrWhiteSpace(choice.NextDialogueId))
            {
                ShowDialogue(choice.NextDialogueId);
                return;
            }

            _interactionUi.SetFeedback($"Bạn chọn: {choice.Text}");
        }

        private void AdvanceDialogue()
        {
            if (_currentDialogue == null ||
                string.IsNullOrWhiteSpace(_currentDialogue.NextDialogueId))
            {
                return;
            }

            ShowDialogue(_currentDialogue.NextDialogueId);
        }

        private void OpenInteraction()
        {
            if (_currentTerminal == null)
            {
                return;
            }

            if (!_dialogueDatabase.TryGetDialogue(
                _currentTerminal.DialogueId,
                out DialogueDefinition dialogue))
            {
                GD.PushWarning(
                    $"Terminal '{_currentTerminal.Name}' references missing dialogue ID '{_currentTerminal.DialogueId}'."
                );
                return;
            }

            _isInteractionOpen = true;
            ShowDialogue(dialogue);
            _interactionUi.SetPromptVisible(false);
            _playerController.SetPhysicsProcess(false);
        }

        private void OpenShop()
        {
            if (!_shopController.Open())
            {
                _interactionUi.SetFeedback("Không thể mở cửa hàng.");
                return;
            }

            _interactionUi.HideInteraction();
        }

        private void ReturnToDialogueMenu()
        {
            _shopController.Close();
            ShowDialogue("terminal_intro");
        }

        private void ShowDialogue(string dialogueId)
        {
            if (!_dialogueDatabase.TryGetDialogue(dialogueId, out DialogueDefinition dialogue))
            {
                GD.PushWarning($"Dialogue references missing nextDialogueId '{dialogueId}'.");
                return;
            }

            ShowDialogue(dialogue);
        }

        private void ShowDialogue(DialogueDefinition dialogue)
        {
            _currentDialogue = dialogue;
            _choiceOptions = dialogue.Choices;
            _selectedChoiceIndex = 0;

            List<string> choiceTexts = new();
            foreach (DialogueChoiceDefinition choice in _choiceOptions)
            {
                choiceTexts.Add(choice.Text);
            }

            _interactionUi.ShowInteraction(
                dialogue.Title,
                dialogue.Message,
                choiceTexts,
                _selectedChoiceIndex
            );
        }

        private void CloseInteraction()
        {
            _isInteractionOpen = false;
            _currentDialogue = null;
            _choiceOptions = Array.Empty<DialogueChoiceDefinition>();
            _shopController.Close();
            _interactionUi.HideInteraction();
            _playerController.SetPhysicsProcess(true);
            _interactionUi.SetPromptVisible(_currentTerminal != null);
        }
    }
}
