using Combat.Entities;
using Combat.Targeting;
using UnityEngine;

namespace UI.Combat
{
    public class CardTargetingAdvanceController
    {
        private readonly CardTargetingSessionState state;
        private readonly CardTargetingHoverController hoverController;

        public CardTargetingAdvanceController(
            CardTargetingSessionState state,
            CardTargetingHoverController hoverController
        )
        {
            this.state = state;
            this.hoverController = hoverController;
        }

        public void ApplyRequestState(ManualTargetRequest request)
        {
            state.validTargets.Clear();
            state.validSpaces.Clear();

            if (request == null)
                return;

            for (int i = 0; i < request.validTargets.Count; i++)
            {
                EntityInstance target = request.validTargets[i];
                if (target != null)
                    state.validTargets.Add(target);
            }

            for (int i = 0; i < request.validSpaces.Count; i++)
            {
                state.validSpaces.Add(request.validSpaces[i]);
            }
        }

        public bool TryAdvanceWithEntity()
        {
            if (state.latchedEntry == null || state.latchedEntry.BoundEntity == null)
            {
                Debug.Log("TARGET ADVANCE ENTITY FAIL | no latched entry");
                return false;
            }

            EntityInstance chosenEntity = state.latchedEntry.BoundEntity;

            if (state.currentRequest == null || state.card == null || state.card.card == null)
            {
                Debug.Log("TARGET ADVANCE ENTITY FAIL | missing request/card");
                return false;
            }

            Debug.Log(
                $"TARGET ADVANCE ENTITY START | card={state.card.card.displayName} | chosen={chosenEntity.id} | stepIndex={state.currentRequest.manualStepIndex}"
            );

            if (!TargetingDecisionService.TryAppendChosenEntityAndGetNextRequest(
                state.owner.CombatRunner.State,
                state.sourceHero,
                state.card.card.targeting,
                state.card.card.effects,
                state.partialTargets,
                state.currentRequest,
                chosenEntity,
                out TargetResult updatedPartialTargets,
                out ManualTargetRequest nextRequest,
                out bool complete
            ))
            {
                Debug.Log("TARGET ADVANCE ENTITY FAIL | TryAppendChosenEntityAndGetNextRequest returned false");
                return false;
            }

            state.partialTargets = updatedPartialTargets;
            state.lockedEntity = chosenEntity;

            Debug.Log(
                $"TARGET ADVANCE ENTITY APPEND OK | complete={complete} | nextRequestNull={(nextRequest == null)}"
            );

            if (complete)
            {
                bool played = state.owner.TryPlayCard(state.card, state.partialTargets);
                Debug.Log($"TARGET ADVANCE ENTITY COMPLETE | owner.TryPlayCard returned {played}");
                return played;
            }

            if (nextRequest == null)
            {
                Debug.Log("TARGET ADVANCE ENTITY FAIL | not complete but nextRequest is null");
                return false;
            }

            state.currentRequest = nextRequest;
            hoverController.ClearHoverState();
            ApplyRequestState(state.currentRequest);

            Debug.Log("TARGET ADVANCE ENTITY SUCCESS | moved to next request");
            return true;
        }

        public bool TryAdvanceWithSpace()
        {
            if (state.latchedSpace == null)
            {
                Debug.Log("TARGET ADVANCE SPACE FAIL | no latched space");
                return false;
            }

            int chosenSpace = state.latchedSpace.SpaceIndex;

            if (state.currentRequest == null || state.card == null || state.card.card == null)
            {
                Debug.Log("TARGET ADVANCE SPACE FAIL | missing request/card");
                return false;
            }

            Debug.Log(
                $"TARGET ADVANCE SPACE START | card={state.card.card.displayName} | chosenSpace={chosenSpace} | stepIndex={state.currentRequest.manualStepIndex}"
            );

            if (!TargetingDecisionService.TryAppendChosenSpaceAndGetNextRequest(
                state.owner.CombatRunner.State,
                state.sourceHero,
                state.card.card.targeting,
                state.card.card.effects,
                state.partialTargets,
                state.currentRequest,
                chosenSpace,
                out TargetResult updatedPartialTargets,
                out ManualTargetRequest nextRequest,
                out bool complete
            ))
            {
                Debug.Log("TARGET ADVANCE SPACE FAIL | TryAppendChosenSpaceAndGetNextRequest returned false");
                return false;
            }

            state.partialTargets = updatedPartialTargets;

            Debug.Log(
                $"TARGET ADVANCE SPACE APPEND OK | complete={complete} | nextRequestNull={(nextRequest == null)}"
            );

            if (complete)
            {
                bool played = state.owner.TryPlayCard(state.card, state.partialTargets);
                Debug.Log($"TARGET ADVANCE SPACE COMPLETE | owner.TryPlayCard returned {played}");
                return played;
            }

            if (nextRequest == null)
            {
                Debug.Log("TARGET ADVANCE SPACE FAIL | not complete but nextRequest is null");
                return false;
            }

            state.currentRequest = nextRequest;
            hoverController.ClearHoverState();
            ApplyRequestState(state.currentRequest);

            Debug.Log("TARGET ADVANCE SPACE SUCCESS | moved to next request");
            return true;
        }
    }
}