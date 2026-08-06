using UnityEngine;
using UnityEngine.InputSystem;
using Combat.Cards;
using Combat.Entities;
using Combat.Targeting;

namespace UI.Combat
{
    public class CardTargetingSessionUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TargetArrowUI arrowUI;

        private CardTargetingSessionState state;
        private CardTargetingHoverController hoverController;
        private CardTargetingVisualController visualController;
        private CardTargetingAdvanceController advanceController;

        public bool IsActive => state != null && state.IsActive;

        private void Awake()
        {
            state = new CardTargetingSessionState();
            hoverController = new CardTargetingHoverController(state);
            visualController = new CardTargetingVisualController(state, arrowUI);
            advanceController = new CardTargetingAdvanceController(state, hoverController);

            visualController.HideArrow();
        }

        private void Update()
        {
            if (!IsActive)
                return;

            Mouse mouse = Mouse.current;
            Keyboard keyboard = Keyboard.current;

            hoverController.UpdateLatchedTarget();

            Vector2 mouseScreen = mouse != null ? mouse.position.ReadValue() : Vector2.zero;
            visualController.UpdateArrow(mouseScreen);

            if (mouse != null && mouse.leftButton.wasReleasedThisFrame)
            {
                string hoveredEntryName = state.hoveredEntry != null && state.hoveredEntry.BoundEntity != null
                    ? state.hoveredEntry.BoundEntity.id
                    : "NULL";

                string latchedEntryName = state.latchedEntry != null && state.latchedEntry.BoundEntity != null
                    ? state.latchedEntry.BoundEntity.id
                    : "NULL";

                int hoveredSpace = state.hoveredSpace != null ? state.hoveredSpace.SpaceIndex : -1;
                int latchedSpace = state.latchedSpace != null ? state.latchedSpace.SpaceIndex : -1;

                Debug.Log(
                    $"TARGET RELEASE FRAME | card={(state.card != null && state.card.card != null ? state.card.card.displayName : "NULL")} | " +
                    $"mode={(state.IsSpaceRequest ? "Space" : "Entity")} | " +
                    $"hoveredEntry={hoveredEntryName} | latchedEntry={latchedEntryName} | " +
                    $"hoveredSpace={hoveredSpace} | latchedSpace={latchedSpace}"
                );

                bool advanced = false;

                if (state.IsSpaceRequest)
                {
                    if (state.latchedSpace != null && state.validSpaces.Contains(state.latchedSpace.SpaceIndex))
                    {
                        Debug.Log($"TARGET RELEASE TRY ADVANCE SPACE | {state.latchedSpace.SpaceIndex}");
                        advanced = advanceController.TryAdvanceWithSpace();
                    }
                    else
                    {
                        Debug.Log("TARGET RELEASE NO VALID LATCHED SPACE");
                    }
                }
                else
                {
                    if (state.latchedEntry != null &&
                        state.latchedEntry.BoundEntity != null &&
                        state.validTargets.Contains(state.latchedEntry.BoundEntity))
                    {
                        Debug.Log($"TARGET RELEASE TRY ADVANCE ENTITY | {state.latchedEntry.BoundEntity.id}");
                        advanced = advanceController.TryAdvanceWithEntity();
                    }
                    else
                    {
                        Debug.Log("TARGET RELEASE NO VALID LATCHED ENTITY");
                    }
                }

                Debug.Log($"TARGET RELEASE RESULT | advanced={advanced}");

                if (advanced)
                {
                    if (!state.IsActive || state.card == null || !state.owner.GetCardContainer().gameObject.activeInHierarchy)
                    {
                        Debug.Log("TARGET RELEASE END SESSION AFTER ADVANCE");
                        EndSession();
                        return;
                    }

                    if (state.currentRequest != null)
                        visualController.RefreshLaneHighlights();

                    return;
                }

                if (IsAwaitingManaSelection())
                {
                    Debug.Log("TARGET RELEASE END SESSION | mana selection took over");
                    EndSession();
                    return;
                }

                Debug.Log("TARGET RELEASE FALLING TO CANCEL");
                CancelSession();
                return;
            }

            if ((mouse != null && mouse.rightButton.wasPressedThisFrame) ||
                (keyboard != null && keyboard.escapeKey.wasPressedThisFrame))
            {
                Debug.Log("TARGET SESSION CANCEL INPUT");
                CancelSession();
            }
        }

        public bool Begin(
            HeroHandUI owner,
            HeroInstance sourceHero,
            CardInstance card,
            HandCardUI sourceCardUI,
            ManualTargetRequest request
        )
        {
            if (owner == null || sourceHero == null || card == null || sourceCardUI == null || request == null)
                return false;

            CancelSession();

            state.owner = owner;
            state.sourceHero = sourceHero;
            state.card = card;
            state.sourceCardUI = sourceCardUI;
            state.currentRequest = request;
            state.partialTargets = request.resolvedSoFar != null ? request.resolvedSoFar : new TargetResult();
            state.lockedEntity = null;

            state.sourceFanLayout = sourceCardUI.GetComponentInParent<HandFanLayout>();
            if (state.sourceFanLayout != null)
                state.sourceFanLayout.SetAimingCard(sourceCardUI);

            state.hoveredEntry = null;
            state.latchedEntry = null;
            state.hoveredSpace = null;
            state.latchedSpace = null;

            advanceController.ApplyRequestState(request);
            visualController.ShowArrow();
            visualController.RefreshLaneHighlights();

            Debug.Log(
                $"TARGET SESSION START | Card: {card.card.displayName} | " +
                $"Mode: {(state.IsSpaceRequest ? "Space" : "Entity")} | " +
                $"Valid entities: {state.validTargets.Count} | Valid spaces: {state.validSpaces.Count}"
            );

            return true;
        }

        public void CancelSession()
        {
            if (state == null || (state.card == null && state.currentRequest == null))
                return;

            if (state.card != null && state.card.card != null)
                Debug.Log($"TARGET SESSION CANCEL | Card: {state.card.card.displayName}");

            EndSession();
        }

        public void HandleLaneEntryPointerEnter(CombatLaneEntryUI entry)
        {
            hoverController.HandleLaneEntryPointerEnter(entry);
            visualController.RefreshLaneHighlights();
        }

        public void HandleLaneEntryPointerExit(CombatLaneEntryUI entry)
        {
            hoverController.HandleLaneEntryPointerExit(entry);
            visualController.RefreshLaneHighlights();
        }

        public void HandleLaneSpacePointerEnter(CombatLaneSpaceUI space)
        {
            hoverController.HandleLaneSpacePointerEnter(space);
            visualController.RefreshLaneHighlights();
        }

        public void HandleLaneSpacePointerExit(CombatLaneSpaceUI space)
        {
            hoverController.HandleLaneSpacePointerExit(space);
            visualController.RefreshLaneHighlights();
        }

        private bool IsAwaitingManaSelection()
        {
            if (state == null || state.owner == null)
                return false;

            HeroHandManaSelectionController manaController = state.owner.GetManaSelectionController();
            return manaController != null && manaController.IsAwaitingSelection;
        }

        private void EndSession()
        {
            if (state.sourceFanLayout != null && state.sourceCardUI != null)
                state.sourceFanLayout.ClearAimingCard(state.sourceCardUI);

            visualController.ClearLaneHighlights();
            visualController.HideArrow();
            state.Reset();
        }
    }
}