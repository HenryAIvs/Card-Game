using UnityEngine;
using Combat.Entities;

namespace UI.Combat
{
    public class CardTargetingVisualController
    {
        private readonly CardTargetingSessionState state;
        private readonly TargetArrowUI arrowUI;

        public CardTargetingVisualController(
            CardTargetingSessionState state,
            TargetArrowUI arrowUI
        )
        {
            this.state = state;
            this.arrowUI = arrowUI;
        }

        public void ShowArrow()
        {
            if (arrowUI != null)
                arrowUI.Show();
        }

        public void HideArrow()
        {
            if (arrowUI != null)
                arrowUI.Hide();
        }

        public void UpdateArrow(Vector2 mouseScreen)
        {
            if (arrowUI == null)
                return;

            Vector2 startScreen = mouseScreen;
            Vector2 endScreen = mouseScreen;

            if (state.IsSpaceRequest)
            {
                if (state.lockedEntity != null)
                {
                    CombatLaneEntryUI lockedEntryUI = FindLaneEntryForEntity(state.lockedEntity);
                    if (lockedEntryUI != null)
                        startScreen = lockedEntryUI.GetTargetPointScreenSpace();
                    else if (state.sourceCardUI != null && state.sourceCardUI.ArrowOrigin != null)
                        startScreen = RectTransformUtility.WorldToScreenPoint(null, state.sourceCardUI.ArrowOrigin.position);
                }
                else if (state.sourceCardUI != null && state.sourceCardUI.ArrowOrigin != null)
                {
                    startScreen = RectTransformUtility.WorldToScreenPoint(null, state.sourceCardUI.ArrowOrigin.position);
                }

                if (state.latchedSpace != null)
                    endScreen = state.latchedSpace.GetTargetPointScreenSpace();
            }
            else
            {
                if (state.sourceCardUI == null || state.sourceCardUI.ArrowOrigin == null)
                    return;

                startScreen = RectTransformUtility.WorldToScreenPoint(null, state.sourceCardUI.ArrowOrigin.position);

                if (state.latchedEntry != null)
                    endScreen = state.latchedEntry.GetTargetPointScreenSpace();
            }

            arrowUI.SetEndpointsScreenSpace(startScreen, endScreen);
        }

        public void RefreshLaneHighlights()
        {
            CombatLaneEntryUI[] entries = Object.FindObjectsByType<CombatLaneEntryUI>(FindObjectsSortMode.None);
            for (int i = 0; i < entries.Length; i++)
            {
                CombatLaneEntryUI entry = entries[i];
                if (entry == null)
                    continue;

                bool isValid = !state.IsSpaceRequest &&
                               entry.BoundEntity != null &&
                               state.validTargets.Contains(entry.BoundEntity);

                bool isLatched = !state.IsSpaceRequest && entry == state.latchedEntry;
                entry.SetTargetingState(isValid, isLatched);
            }

            CombatLaneSpaceUI[] spaces = Object.FindObjectsByType<CombatLaneSpaceUI>(FindObjectsSortMode.None);
            for (int i = 0; i < spaces.Length; i++)
            {
                CombatLaneSpaceUI space = spaces[i];
                if (space == null)
                    continue;

                bool isValid = state.IsSpaceRequest && state.validSpaces.Contains(space.SpaceIndex);
                bool isLatched = state.IsSpaceRequest && space == state.latchedSpace;
                space.SetTargetingState(isValid, isLatched);
            }

            if (state.hoveredSpace != null && !state.validSpaces.Contains(state.hoveredSpace.SpaceIndex))
                state.hoveredSpace = null;

            if (state.latchedSpace != null && !state.validSpaces.Contains(state.latchedSpace.SpaceIndex))
                state.latchedSpace = null;

            if (state.hoveredEntry != null &&
                (state.hoveredEntry.BoundEntity == null || !state.validTargets.Contains(state.hoveredEntry.BoundEntity)))
            {
                state.hoveredEntry = null;
            }

            if (state.latchedEntry != null &&
                (state.latchedEntry.BoundEntity == null || !state.validTargets.Contains(state.latchedEntry.BoundEntity)))
            {
                state.latchedEntry = null;
            }
        }

        public void ClearLaneHighlights()
        {
            CombatLaneEntryUI[] entries = Object.FindObjectsByType<CombatLaneEntryUI>(FindObjectsSortMode.None);
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i] != null)
                    entries[i].SetTargetingState(false, false);
            }

            CombatLaneSpaceUI[] spaces = Object.FindObjectsByType<CombatLaneSpaceUI>(FindObjectsSortMode.None);
            for (int i = 0; i < spaces.Length; i++)
            {
                if (spaces[i] != null)
                    spaces[i].SetTargetingState(false, false);
            }
        }

        private CombatLaneEntryUI FindLaneEntryForEntity(EntityInstance entity)
        {
            if (entity == null)
                return null;

            CombatLaneEntryUI[] entries = Object.FindObjectsByType<CombatLaneEntryUI>(FindObjectsSortMode.None);
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i] != null && entries[i].BoundEntity == entity)
                    return entries[i];
            }

            return null;
        }
    }
}