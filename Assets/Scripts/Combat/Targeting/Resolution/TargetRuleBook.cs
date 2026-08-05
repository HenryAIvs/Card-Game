using System.Collections.Generic;
using System.Linq;
using Combat.Conditions;
using Combat.Entities;
using Combat.Statuses;

namespace Combat.Targeting
{
    public static class TargetRuleBook
    {
        public static List<EntityInstance> ApplyEntityRules(
            TargetContext context,
            List<EntityInstance> candidates
        )
        {
            if (context == null || context.state == null || context.step == null)
                return new List<EntityInstance>();

            candidates = candidates
                .Where(e => e != null && !context.state.conditions.Has(e, ConditionIds.Unconscious))
                .ToList();

            bool isChosenSelection =
                context.step.specifier == TargetSpecifier.Any ||
                context.step.specifier == TargetSpecifier.PickX;

            if (context.step.filter == TargetFilter.Adversary && isChosenSelection)
            {
                candidates = candidates
                    .Where(e => context.state.statuses.Get(e, StatusId.Hidden) <= 0)
                    .ToList();
            }

            return candidates;
        }
    }
}