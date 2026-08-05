using System.Collections.Generic;
using Combat.Core;
using Combat.Entities;
using Combat.Data.Effects;

namespace Combat.Targeting
{
    public static class TargetResolver
    {
        public delegate List<EntityInstance> ChooseEntitiesFn(List<EntityInstance> options, int count);
        public delegate List<int> ChooseSpacesFn(List<int> options, int count);

        public static TargetResult ResolveSequence(
            CombatState state,
            EntityInstance source,
            List<TargetStep> steps,
            List<EffectSO> effects,
            ChooseEntitiesFn chooseEntities,
            ChooseSpacesFn chooseSpaces
        )
        {
            TargetResult result = new TargetResult();

            if (steps == null)
                return result;

            for (int i = 0; i < steps.Count; i++)
            {
                ResolveStep(state, source, steps[i], result, effects, chooseEntities, chooseSpaces);
            }

            return result;
        }

        public static void ResolveStep(
            CombatState state,
            EntityInstance source,
            TargetStep step,
            TargetResult result,
            List<EffectSO> effects,
            ChooseEntitiesFn chooseEntities,
            ChooseSpacesFn chooseSpaces
        )
        {
            if (step == null)
                return;

            TargetContext context = new TargetContext(state, source, step, result, effects);

            List<EntityInstance> entityCandidates = TargetCandidateBuilder.BuildEntityCandidates(context);
            entityCandidates = TargetRuleBook.ApplyEntityRules(context, entityCandidates);

            List<int> spaceCandidates = step.filter == TargetFilter.Space
                ? TargetCandidateBuilder.BuildSpaceCandidates(context)
                : new List<int>();

            ITargetMode mode = TargetModeRegistry.GetMode(step);
            mode.Resolve(context, entityCandidates, spaceCandidates, chooseEntities, chooseSpaces);
        }
    }
}