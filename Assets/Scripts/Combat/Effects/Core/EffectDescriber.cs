using System.Collections.Generic;

namespace Combat.Data.Effects
{
    // Short human-readable summaries of effects for UI (enemy turn banners,
    // target choice buttons).
    public static class EffectDescriber
    {
        public static string Describe(EffectSO effect)
        {
            switch (effect)
            {
                case null:
                    return "";

                case HitEffectSO hit:
                    return $"Hit {hit.amount}";

                case MoveUpToEffectSO move:
                    return $"Move {move.maxDistance}";

                case ChangeStatusEffectSO status:
                    return $"{status.status} {status.amount}";

                case ApplyConditionEffectSO condition:
                    return Capitalise(condition.conditionId);

                case TargetChoiceEffectSO choice:
                    return DescribeChoice(choice);

                default:
                    return effect.Keyword;
            }
        }

        public static string DescribeList(List<EffectSO> effects)
        {
            if (effects == null || effects.Count == 0)
                return "";

            var parts = new List<string>();

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] == null)
                    continue;

                parts.Add(Describe(effects[i]));
            }

            return string.Join(", ", parts);
        }

        private static string DescribeChoice(TargetChoiceEffectSO choice)
        {
            if (choice.options == null || choice.options.Count == 0)
                return choice.Keyword;

            var parts = new List<string>();

            for (int i = 0; i < choice.options.Count; i++)
            {
                if (choice.options[i] == null)
                    continue;

                parts.Add(Describe(choice.options[i]));
            }

            return $"Target chooses: {string.Join(" or ", parts)}";
        }

        private static string Capitalise(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }
    }
}
