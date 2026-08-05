using System;
using System.Collections.Generic;
using Combat.Core;
using Combat.Data.Effects;
using Combat.Entities;
using Combat.Targeting;

namespace Combat.Resolution
{
    public static class EffectExecutor
    {
        public static void ExecuteAbility(
            CombatState state,
            EntityInstance source,
            List<TargetStep> targeting,
            List<EffectSO> effects
        )
        {
            List<EntityInstance> ChooseEntities(List<EntityInstance> options, int count)
            {
                var picked = new List<EntityInstance>();
                for (int i = 0; i < Math.Min(count, options.Count); i++)
                    picked.Add(options[i]);
                return picked;
            }

            List<int> ChooseSpaces(List<int> options, int count)
            {
                var picked = new List<int>();
                for (int i = 0; i < Math.Min(count, options.Count); i++)
                    picked.Add(options[i]);
                return picked;
            }

            var targets = TargetResolver.ResolveSequence(
                state,
                source,
                targeting,
                effects,
                ChooseEntities,
                ChooseSpaces
            );

            ExecuteAbility(state, source, targets, effects);
        }

        public static void ExecuteAbility(
            CombatState state,
            EntityInstance source,
            TargetResult targets,
            List<EffectSO> effects
        )
        {
            if (state == null || source == null || effects == null)
                return;

            if (targets == null)
                targets = new TargetResult();

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] == null)
                    continue;

                effects[i].Execute(state, source, targets);

                if (state.IsCombatOver)
                    return;
            }
        }
    }
}