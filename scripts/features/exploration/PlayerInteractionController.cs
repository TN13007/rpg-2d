using System.Collections.Generic;
using Godot;
using RPG2D.Features.Interaction;

namespace RPG2D.Features.Exploration
{
    public partial class PlayerInteractionController : Node
    {
        private sealed class ChoiceDefinition
        {
            public string Text { get; }
            public bool ClosesInteraction { get; }

            public ChoiceDefinition(string text, bool closesInteraction = false)
            {
                Text = text;
                ClosesInteraction = closesInteraction;
            }
        }

        private PlayerController _playerController = null!;
        private TerminalController? _currentTerminal;
        private TerminalInteractionUI _interactionUi = null!;

        private readonly List<ChoiceDefinition> _choiceOptions = new()
        {
            new ChoiceDefinition("Lựa chọn 1"),
            new ChoiceDefinition("Lựa chọn 2"),
            new ChoiceDefinition("Lựa chọn 3"),
            new ChoiceDefinition("Lựa chọn 4"),
            new ChoiceDefinition("Lựa chọn 5"),
            new ChoiceDefinition("Lựa chọn 6"),
            new ChoiceDefinition("Leave", true)
        };

        private int _selectedChoiceIndex;
        private bool _isInteractionOpen;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            _playerController = GetParent<PlayerController>();

            _interactionUi = new TerminalInteractionUI();
            AddChild(_interactionUi);
            _interactionUi.ChoiceHovered += SelectChoice;
            _interactionUi.ChoiceConfirmed += OnChoiceConfirmed;
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
            SelectChoice(index);

            if (index >= 0 && index < _choiceOptions.Count)
            {
                ConfirmSelection();
            }
        }

        private void ConfirmSelection()
        {
            if (_selectedChoiceIndex < 0 ||
                _selectedChoiceIndex >= _choiceOptions.Count)
            {
                return;
            }

            if (_choiceOptions[_selectedChoiceIndex].ClosesInteraction)
            {
                CloseInteraction();
                return;
            }

            _interactionUi.SetFeedback($"Bạn chọn số {_selectedChoiceIndex + 1}");
        }

        private void OpenInteraction()
        {
            if (_currentTerminal == null)
            {
                return;
            }

            _isInteractionOpen = true;
            _selectedChoiceIndex = 0;

            List<string> choiceTexts = new();
            foreach (ChoiceDefinition option in _choiceOptions)
            {
                choiceTexts.Add(option.Text);
            }

            _interactionUi.ShowInteraction(
                _currentTerminal.DisplayName,
                _currentTerminal.Response,
                choiceTexts,
                _selectedChoiceIndex
            );
            _interactionUi.SetPromptVisible(false);
            _playerController.SetPhysicsProcess(false);
        }

        private void CloseInteraction()
        {
            _isInteractionOpen = false;
            _interactionUi.HideInteraction();
            _playerController.SetPhysicsProcess(true);
            _interactionUi.SetPromptVisible(_currentTerminal != null);
        }
    }
}
