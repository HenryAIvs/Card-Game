using UnityEngine;

namespace UI.Combat
{
    public class CardTargetingHoverController
    {
        private readonly CardTargetingSessionState state;

        public CardTargetingHoverController(CardTargetingSessionState state)
        {
            this.state = state;
        }

        public void HandleLaneEntryPointerEnter(CombatLaneEntryUI entry)
        {
            if (!state.IsActive || state.IsSpaceRequest || entry == null)
                return;

            string entryName = entry.BoundEntity != null ? entry.BoundEntity.id : "NULL";
            bool valid = entry.BoundEntity != null && state.validTargets.Contains(entry.BoundEntity);

            Debug.Log($"TARGET HOVER ENTER | entry={entryName} | valid={valid}");

            if (!valid)
            {
                if (state.hoveredEntry == entry)
                    state.hoveredEntry = null;

                UpdateLatchedTarget();
                return;
            }

            state.hoveredEntry = entry;
            UpdateLatchedTarget();
        }

        public void HandleLaneEntryPointerExit(CombatLaneEntryUI entry)
        {
            if (!state.IsActive || state.IsSpaceRequest)
                return;

            string entryName = entry != null && entry.BoundEntity != null ? entry.BoundEntity.id : "NULL";
            Debug.Log($"TARGET HOVER EXIT | entry={entryName}");

            if (state.hoveredEntry == entry)
                state.hoveredEntry = null;

            // Un-latch so releasing over empty space cancels instead of
            // firing at the last hovered target.
            if (state.latchedEntry == entry)
                state.latchedEntry = null;

            UpdateLatchedTarget();
        }

        public void HandleLaneSpacePointerEnter(CombatLaneSpaceUI space)
        {
            if (!state.IsActive || !state.IsSpaceRequest || space == null)
                return;

            bool valid = state.validSpaces.Contains(space.SpaceIndex);
            Debug.Log($"TARGET SPACE ENTER | space={space.SpaceIndex} | valid={valid}");

            if (!valid)
            {
                if (state.hoveredSpace == space)
                    state.hoveredSpace = null;

                UpdateLatchedTarget();
                return;
            }

            state.hoveredSpace = space;
            UpdateLatchedTarget();
        }

        public void HandleLaneSpacePointerExit(CombatLaneSpaceUI space)
        {
            if (!state.IsActive || !state.IsSpaceRequest)
                return;

            int spaceIndex = space != null ? space.SpaceIndex : -1;
            Debug.Log($"TARGET SPACE EXIT | space={spaceIndex}");

            if (state.hoveredSpace == space)
                state.hoveredSpace = null;

            if (state.latchedSpace == space)
                state.latchedSpace = null;

            UpdateLatchedTarget();
        }

        public void ClearHoverState()
        {
            Debug.Log("TARGET HOVER CLEAR");

            state.hoveredEntry = null;
            state.latchedEntry = null;
            state.hoveredSpace = null;
            state.latchedSpace = null;
        }

        public void UpdateLatchedTarget()
        {
            // Runs every frame during a session - only log when the latch changes.
            if (state.IsSpaceRequest)
            {
                CombatLaneSpaceUI beforeLatched = state.latchedSpace;

                if (state.hoveredSpace != null && state.validSpaces.Contains(state.hoveredSpace.SpaceIndex))
                {
                    state.latchedSpace = state.hoveredSpace;
                }
                else if (state.latchedSpace != null && !state.validSpaces.Contains(state.latchedSpace.SpaceIndex))
                {
                    state.latchedSpace = null;
                }

                if (state.latchedSpace != beforeLatched)
                {
                    Debug.Log(
                        $"TARGET LATCH UPDATE SPACE | latched={(state.latchedSpace != null ? state.latchedSpace.SpaceIndex : -1)}"
                    );
                }

                state.latchedEntry = null;
                return;
            }

            CombatLaneEntryUI beforeLatchedEntry = state.latchedEntry;

            if (state.hoveredEntry != null &&
                state.hoveredEntry.BoundEntity != null &&
                state.validTargets.Contains(state.hoveredEntry.BoundEntity))
            {
                state.latchedEntry = state.hoveredEntry;
            }
            else if (state.latchedEntry != null &&
                     (state.latchedEntry.BoundEntity == null ||
                      !state.validTargets.Contains(state.latchedEntry.BoundEntity)))
            {
                state.latchedEntry = null;
            }

            if (state.latchedEntry != beforeLatchedEntry)
            {
                string latchedName = state.latchedEntry != null && state.latchedEntry.BoundEntity != null
                    ? state.latchedEntry.BoundEntity.id
                    : "NULL";

                Debug.Log($"TARGET LATCH UPDATE ENTITY | latched={latchedName}");
            }

            state.latchedSpace = null;
        }
    }
}