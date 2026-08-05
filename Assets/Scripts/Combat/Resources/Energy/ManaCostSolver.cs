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

        // Can the cost be paid at all, with optimal colour assignments?
        public static bool CanPay(EnergyPool available, ManaCost cost)
        {
            return CanPayFrom(available, FlattenPips(cost), 0);
        }

        public static bool TryBuildPaymentPlan(
            EnergyPool available,
            ManaCost cost,
            ChooseManaColorFn chooser,
            out PaymentPlan plan
        )
        {
            plan = new PaymentPlan();

            List<ManaColor[]> pips = FlattenPips(cost);

            for (int i = 0; i < pips.Count; i++)
            {
                // Colours that are in stock AND still leave the remaining pips payable.
                List<ManaColor> viable = GetViableColors(available, pips, i);
                if (viable.Count == 0) return false;

                ManaColor chosen = viable[0];

                if (viable.Count > 1 && chooser != null)
                {
                    ManaColor requested = chooser(viable, GetReason(pips[i].Length));
                    if (viable.Contains(requested))
                        chosen = requested;
                }

                available.Spend(chosen, 1);
                ApplySpend(plan, chosen, 1);
            }

            return true;
        }

        private static List<ManaColor> GetViableColors(
            EnergyPool available,
            List<ManaColor[]> pips,
            int index
        )
        {
            var viable = new List<ManaColor>();
            ManaColor[] options = pips[index];

            for (int i = 0; i < options.Length; i++)
            {
                ManaColor color = options[i];
                if (available.Get(color) <= 0) continue;

                EnergyPool next = available;
                next.Spend(color, 1);

                if (CanPayFrom(next, pips, index + 1))
                    viable.Add(color);
            }

            return viable;
        }

        private static bool CanPayFrom(EnergyPool available, List<ManaColor[]> pips, int index)
        {
            if (index >= pips.Count) return true;

            ManaColor[] options = pips[index];

            for (int i = 0; i < options.Length; i++)
            {
                ManaColor color = options[i];
                if (available.Get(color) <= 0) continue;

                EnergyPool next = available;
                next.Spend(color, 1);

                if (CanPayFrom(next, pips, index + 1))
                    return true;
            }

            return false;
        }

        // One entry per pip, each holding its distinct colour options.
        private static List<ManaColor[]> FlattenPips(ManaCost cost)
        {
            var pips = new List<ManaColor[]>();

            if (cost == null || cost.costs == null)
                return pips;

            for (int i = 0; i < cost.costs.Count; i++)
            {
                ChoiceCost chunk = cost.costs[i];

                if (chunk.options == null || chunk.options.Length == 0)
                    throw new ArgumentException("ChoiceCost.options must not be empty.");

                ManaColor[] distinct = chunk.options.Distinct().ToArray();

                for (int pip = 0; pip < chunk.amount; pip++)
                    pips.Add(distinct);
            }

            return pips;
        }

        private static string GetReason(int optionCount)
        {
            return optionCount == 1 ? "forced" :
                   optionCount == 2 ? "flex" :
                   "generic";
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
