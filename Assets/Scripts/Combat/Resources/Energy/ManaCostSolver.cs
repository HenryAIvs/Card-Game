using System;
using System.Collections.Generic;
using System.Linq;

namespace Combat.Resources.Mana
{
    public static class ManaCostSolver
    {
        public delegate ManaColor ChooseManaColorFn(
            List<ManaColor> feasibleOptions,
            string reason
        );

        public static bool TryBuildPaymentPlan(
            EnergyPool available,
            ManaCost cost,
            ChooseManaColorFn chooser,
            out PaymentPlan plan
        )
        {
            plan = new PaymentPlan();

            if (cost == null || cost.costs == null) return true;

            for (int i = 0; i < cost.costs.Count; i++)
            {
                var chunk = cost.costs[i];

                if (chunk.options == null || chunk.options.Length < 1 || chunk.options.Length > 3)
                    throw new ArgumentException("ChoiceCost.options must have length 1 to 3.");

                string reason =
                    chunk.options.Length == 1 ? "forced" :
                    chunk.options.Length == 2 ? "flex" :
                    "generic";

                for (int pip = 0; pip < chunk.amount; pip++)
                {
                    var feasible = chunk.options
                        .Distinct()
                        .Where(c => available.Get(c) > 0)
                        .ToList();

                    if (feasible.Count == 0) return false;

                    ManaColor chosen;
                    if (feasible.Count == 1)
                    {
                        chosen = feasible[0];
                    }
                    else
                    {
                        chosen = chooser != null ? chooser(feasible, reason) : feasible[0];
                        if (!feasible.Contains(chosen))
                            chosen = feasible[0];
                    }

                    available.Spend(chosen, 1);
                    ApplySpend(plan, chosen, 1);
                    plan.choiceLog.Add($"{reason}: paid 1 with {chosen}");
                }
            }

            return true;
        }

        private static void ApplySpend(PaymentPlan plan, ManaColor c, int amount)
        {
            switch (c)
            {
                case ManaColor.Red: plan.spendRed += amount; break;
                case ManaColor.Yellow: plan.spendYellow += amount; break;
                case ManaColor.Blue: plan.spendBlue += amount; break;
            }
        }
    }
}
