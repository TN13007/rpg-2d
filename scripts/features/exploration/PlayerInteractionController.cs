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
        private const float InteractionRange = 20.0f;
        private const uint WorldCollisionMask = 1u;
        private const uint InteractionCollisionMask = 2u;

        private PlayerController _playerController = null!;
        private PlayerAnimationController _playerAnimationController = null!;
        private ShapeCast2D _interactionShapeCast = null!;
        private DialogueInteractableController? _currentInteractable;
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
            _playerAnimationController =
                _playerController.GetNode<PlayerAnimationController>("PlayerAnimationController");
            _interactionShapeCast = GetNode<ShapeCast2D>("../InteractionShapeCast2D");
            _interactionShapeCast.CollisionMask = WorldCollisionMask | InteractionCollisionMask;
            _interactionShapeCast.CollideWithAreas = true;
            _interactionShapeCast.CollideWithBodies = true;

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

            UpdateCurrentInteractable();

            if (_currentInteractable != null && Input.IsActionJustPressed("interact"))
            {
                OpenInteraction();
            }
        }

        private void UpdateCurrentInteractable()
        {
            _interactionShapeCast.TargetPosition =
                _playerAnimationController.FacingDirection * InteractionRange;
            _interactionShapeCast.ForceShapecastUpdate();

            DialogueInteractableController? interactable = FindNearestInteractable();
            if (_currentInteractable == interactable)
            {
                return;
            }

            _currentInteractable = interactable;
            _interactionUi.SetPromptVisible(_currentInteractable != null);
        }

        private DialogueInteractableController? FindNearestInteractable()
        {
            int collisionCount = _interactionShapeCast.GetCollisionCount();
            if (collisionCount == 0)
            {
                return null;
            }

            int nearestCollisionIndex = -1;
            float nearestDistanceSquared = float.MaxValue;

            for (int index = 0; index < collisionCount; index++)
            {
                Vector2 collisionPoint = _interactionShapeCast.GetCollisionPoint(index);
                float distanceSquared = _interactionShapeCast.GlobalPosition.DistanceSquaredTo(collisionPoint);

                if (distanceSquared < nearestDistanceSquared)
                {
                    nearestDistanceSquared = distanceSquared;
                    nearestCollisionIndex = index;
                }
            }

            if (nearestCollisionIndex < 0 ||
                _interactionShapeCast.GetCollider(nearestCollisionIndex) is not Node collider)
            {
                return null;
            }

            if (collider is Area2D interactionArea &&
                interactionArea.CollisionLayer != InteractionCollisionMask)
            {
                return null;
            }

            return collider.GetParent() as DialogueInteractableController
                ?? collider.GetParent()?.GetParent() as DialogueInteractableController;
        }

        private void HandleInteractionInput()
        {
            if (Input.IsActionJustPressed("ui_cancel") &&
                _currentInteractable?.CanCancelInteraction == true)
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
                if (!DialogueChoiceDefinition.IsSupportedShopMode(choice.ShopMode) ||
                    !Enum.TryParse(choice.ShopMode, true, out ShopMode shopMode) ||
                    !Enum.IsDefined(shopMode))
                {
                    _interactionUi.SetFeedback("Chế độ cửa hàng không hợp lệ.");
                    return;
                }

                OpenShop(shopMode);
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
            if (_currentInteractable == null)
            {
                return;
            }

            if (!_dialogueDatabase.TryGetDialogue(
                _currentInteractable.DialogueId,
                out DialogueDefinition dialogue))
            {
                GD.PushWarning(
                    $"Interactable '{_currentInteractable.Name}' references missing dialogue ID '{_currentInteractable.DialogueId}'."
                );
                return;
            }

            _isInteractionOpen = true;
            ShowDialogue(dialogue);
            _interactionUi.SetPromptVisible(false);
            _playerController.SetPhysicsProcess(false);
        }

        private void OpenShop(ShopMode mode)
        {
            if (!_shopController.Open(mode))
            {
                _interactionUi.SetFeedback("Không thể mở cửa hàng.");
                return;
            }

            _interactionUi.HideInteraction();
        }

        private void ReturnToDialogueMenu()
        {
            _shopController.Close();

            if (_currentDialogue == null)
            {
                CloseInteraction();
                return;
            }

            ShowDialogue(_currentDialogue);
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
            _interactionUi.SetPromptVisible(_currentInteractable != null);
        }
    }
}
