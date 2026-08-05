using System.Collections.Generic;
using Combat.Core;
using Combat.Entities;
using Combat.Data.Effects;
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

        public static bool TryGetFirstManualEntityTargetRequest(
            CombatState state,
            EntityInstance source,
            List<TargetStep> steps,
            List<EffectSO> effects,
            out ManualTargetRequest request
        )
        {
            request = null;

            if (!TryGetFirstManualTargetRequest(state, source, steps, effects, out ManualTargetRequest genericRequest))
                return false;

            if (genericRequest == null || genericRequest.IsSpaceRequest)
                return false;

            request = genericRequest;
            return true;
        }

        public static bool TryBuildChosenTargetsForSingleEntity(
            CombatState state,
            EntityInstance source,
            List<TargetStep> steps,
            List<EffectSO> effects,
            ManualTargetRequest request,
            EntityInstance chosenTarget,
            out TargetResult result
        )
        {
            result = new TargetResult();

            if (state == null || source == null || steps == null || request == null || chosenTarget == null)
                return false;

            if (request.IsSpaceRequest)
                return false;

            for (int i = 0; i < steps.Count; i++)
            {
                TargetStep step = steps[i];
                if (step == null)
                    continue;

                TargetContext context = new TargetContext(state, source, step, result, effects);

                List<EntityInstance> entityCandidates = TargetCandidateBuilder.BuildEntityCandidates(context);
                entityCandidates = TargetRuleBook.ApplyEntityRules(context, entityCandidates);

                List<int> spaceCandidates = step.filter == TargetFilter.Space
                    ? TargetCandidateBuilder.BuildSpaceCandidates(context)
                    : new List<int>();

                ITargetMode mode = TargetModeRegistry.GetMode(step);

                if (i == request.manualStepIndex)
                {
                    if (!entityCandidates.Contains(chosenTarget))
                        return false;

                    result.SetEntities(step.label, new List<EntityInstance> { chosenTarget });
                    continue;
                }

                mode.AutoResolve(context, entityCandidates, spaceCandidates);
            }

            return true;
        }

        public static bool TryBuildChosenTargetsForSingleSpace(
            CombatState state,
            EntityInstance source,
            List<TargetStep> steps,
            List<EffectSO> effects,
            ManualTargetRequest request,
            int chosenSpace,
            out TargetResult result
        )
        {
            result = new TargetResult();

            if (state == null || source == null || steps == null || request == null)
                return false;

            if (!request.IsSpaceRequest)
                return false;

            for (int i = 0; i < steps.Count; i++)
            {
                TargetStep step = steps[i];
                if (step == null)
                    continue;

                TargetContext context = new TargetContext(state, source, step, result, effects);

                List<EntityInstance> entityCandidates = TargetCandidateBuilder.BuildEntityCandidates(context);
                entityCandidates = TargetRuleBook.ApplyEntityRules(context, entityCandidates);

                List<int> spaceCandidates = step.filter == TargetFilter.Space
                    ? TargetCandidateBuilder.BuildSpaceCandidates(context)
                    : new List<int>();

                ITargetMode mode = TargetModeRegistry.GetMode(step);

                if (i == request.manualStepIndex)
                {
                    if (!spaceCandidates.Contains(chosenSpace))
                        return false;

                    result.SetSpaces(step.label, new List<int> { chosenSpace });
                    continue;
                }

                mode.AutoResolve(context, entityCandidates, spaceCandidates);
            }

            return true;
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
                    validTargets = new List<EntityInstance>(entityCandidates)
                };

                return true;
            }

            complete = true;
            return true;
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