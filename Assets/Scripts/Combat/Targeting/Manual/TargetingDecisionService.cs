using System.Collections.Generic;
using Combat.Core;
using Combat.Entities;
using Combat.Data.Effects;
using Combat.Resolution.Types;
using UnityEngine;

namespace Combat.Targeting
{
    public static class TargetingDecisionService
    {
        public static bool RequiresManualTargeting(
            CombatState state,
            EntityInstance source,
            List<TargetStep> steps,
            List<EffectSO> effects
        )
        {
            return TryGetFirstManualTargetRequest(
                state,
                source,
                steps,
                effects,
                out _
            );
        }

        public static bool TryGetFirstManualTargetRequest(
            CombatState state,
            EntityInstance source,
            List<TargetStep> steps,
            List<EffectSO> effects,
            out ManualTargetRequest request
        )
        {
            request = null;

            if (state == null || source == null || steps == null || steps.Count == 0)
                return false;

            TargetResult simulatedResult = new TargetResult();

            bool success = TryGetNextManualTargetRequestFromPartial(
                state,
                source,
                steps,
                effects,
                simulatedResult,
                0,
                out request,
                out bool complete
            );

            if (!success)
                return false;

            return request != null && !complete;
        }

        public static bool TryAppendChosenEntityAndGetNextRequest(
            CombatState state,
            EntityInstance source,
            List<TargetStep> steps,
            List<EffectSO> effects,
            TargetResult partialResult,
            ManualTargetRequest request,
            EntityInstance chosenTarget,
            out TargetResult updatedResult,
            out ManualTargetRequest nextRequest,
            out bool complete
        )
        {
            updatedResult = CloneTargetResult(partialResult);
            nextRequest = null;
            complete = false;

            if (state == null || source == null || steps == null || request == null || chosenTarget == null)
                return false;

            if (request.IsSpaceRequest)
                return false;

            TargetStep step = steps[request.manualStepIndex];
            if (step == null)
                return false;

            TargetContext context = new TargetContext(state, source, step, updatedResult, effects);
            List<EntityInstance> entityCandidates = TargetCandidateBuilder.BuildEntityCandidates(context);
            entityCandidates = TargetRuleBook.ApplyEntityRules(context, entityCandidates);

            if (!entityCandidates.Contains(chosenTarget))
                return false;

            updatedResult.SetEntities(step.label, new List<EntityInstance> { chosenTarget });

            return TryGetNextManualTargetRequestFromPartial(
                state,
                source,
                steps,
                effects,
                updatedResult,
                request.manualStepIndex + 1,
                out nextRequest,
                out complete
            );
        }

        public static bool TryAppendChosenSpaceAndGetNextRequest(
            CombatState state,
            EntityInstance source,
            List<TargetStep> steps,
            List<EffectSO> effects,
            TargetResult partialResult,
            ManualTargetRequest request,
            int chosenSpace,
            out TargetResult updatedResult,
            out ManualTargetRequest nextRequest,
            out bool complete
        )
        {
            updatedResult = CloneTargetResult(partialResult);
            nextRequest = null;
            complete = false;

            if (state == null || source == null || steps == null || request == null)
                return false;

            if (!request.IsSpaceRequest)
                return false;

            TargetStep step = steps[request.manualStepIndex];
            if (step == null)
                return false;

            TargetContext context = new TargetContext(state, source, step, updatedResult, effects);
            List<int> spaceCandidates = step.filter == TargetFilter.Space
                ? TargetCandidateBuilder.BuildSpaceCandidates(context)
                : new List<int>();

            if (!spaceCandidates.Contains(chosenSpace))
                return false;

            updatedResult.SetSpaces(step.label, new List<int> { chosenSpace });

            return TryGetNextManualTargetRequestFromPartial(
                state,
                source,
                steps,
                effects,
                updatedResult,
                request.manualStepIndex + 1,
                out nextRequest,
                out complete
            );
        }

        private static bool TryGetNextManualTargetRequestFromPartial(
            CombatState state,
            EntityInstance source,
            List<TargetStep> steps,
            List<EffectSO> effects,
            TargetResult simulatedResult,
            int startIndex,
            out ManualTargetRequest request,
            out bool complete
        )
        {
            request = null;
            complete = false;

            if (state == null || source == null || steps == null)
                return false;

            for (int i = startIndex; i < steps.Count; i++)
            {
                TargetStep step = steps[i];
                if (step == null)
                    continue;

                if (step.filter == TargetFilter.Space && ShouldSkipSpaceStep(source, effects, step.label))
                {
                    Debug.Log($"TARGET STEP SKIP | space step {step.label} only feeds inactive parity effects");
                    continue;
                }

                TargetContext context = new TargetContext(state, source, step, simulatedResult, effects);

                List<EntityInstance> entityCandidates = TargetCandidateBuilder.BuildEntityCandidates(context);
                entityCandidates = TargetRuleBook.ApplyEntityRules(context, entityCandidates);

                List<int> spaceCandidates = step.filter == TargetFilter.Space
                    ? TargetCandidateBuilder.BuildSpaceCandidates(context)
                    : new List<int>();

                ITargetMode mode = TargetModeRegistry.GetMode(step);
                bool needsManual = mode.RequiresManualChoice(context, entityCandidates, spaceCandidates);

                if (!needsManual)
                {
                    mode.AutoResolve(context, entityCandidates, spaceCandidates);
                    continue;
                }

                if (step.specifier == TargetSpecifier.PickX && Mathf.Max(1, step.pickCount) > 1)
                    return false;

                if (step.filter == TargetFilter.Space)
                {
                    if (spaceCandidates.Count == 0)
                        return false;

                    request = new ManualTargetRequest
                    {
                        manualStepIndex = i,
                        step = step,
                        resolvedSoFar = simulatedResult,
                        validSpaces = new List<int>(spaceCandidates)
                    };

                    return true;
                }

                if (entityCandidates.Count == 0)
                    return false;

                request = new ManualTargetRequest
                {
                    manualStepIndex = i,
                    step = step,
                    resolvedSoFar = simulatedResult,
                    validTargets = new List<EntityInstance>(entityCandidates)
                };

                return true;
            }

            complete = true;
            return true;
        }

        // A space step is skippable when the moves consuming its label all sit
        // behind parity conditions the current hand size fails. If no known
        // effect references the label at all, keep the step (be conservative).
        private static bool ShouldSkipSpaceStep(
            EntityInstance source,
            List<EffectSO> effects,
            string label
        )
        {
            if (!SpaceLabelReferenced(effects, label, source, activeOnly: false))
                return false;

            return !SpaceLabelReferenced(effects, label, source, activeOnly: true);
        }

        private static bool SpaceLabelReferenced(
            List<EffectSO> effects,
            string label,
            EntityInstance source,
            bool activeOnly
        )
        {
            if (effects == null)
                return false;

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] is MoveUpToEffectSO moveEffect && moveEffect.spaceLabel == label)
                    return true;

                if (effects[i] is ParityConditionalEffectSO conditional)
                {
                    if (activeOnly && !IsParityConditionMet(conditional, source))
                        continue;

                    if (SpaceLabelReferenced(conditional.effects, label, source, activeOnly))
                        return true;
                }
            }

            return false;
        }

        private static bool IsParityConditionMet(ParityConditionalEffectSO conditional, EntityInstance source)
        {
            if (conditional == null || source is not HeroInstance hero)
                return true;

            bool isOdd = (hero.deck.hand.Count % 2) == 1;
            return conditional.parity == ParityType.Left ? isOdd : !isOdd;
        }

        private static TargetResult CloneTargetResult(TargetResult input)
        {
            TargetResult copy = new TargetResult();

            if (input == null)
                return copy;

            foreach (var pair in input.selections)
            {
                copy.selections[pair.Key] = new List<EntityInstance>(pair.Value);
            }

            foreach (var pair in input.spaces)
            {
                copy.spaces[pair.Key] = new List<int>(pair.Value);
            }

            return copy;
        }
    }
}