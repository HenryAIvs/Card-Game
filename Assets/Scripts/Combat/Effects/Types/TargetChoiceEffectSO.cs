using System.Collections.Generic;
using UnityEngine;
using Combat.Core;
using Combat.Entities;
using Combat.Targeting;

namespace Combat.Data.Effects
{
    [CreateAssetMenu(menuName = "Combat/Effects/Target Choice", fileName = "Effect_TargetChoice")]
    public class TargetChoiceEffectSO : EffectSO
    {
        public override string Keyword => "TargetChoice";

        // Which targets are affected by this choice (usually T1)
        public string targetLabel = "T1";

        // Effects the TARGET chooses between
        public List<EffectSO> options = new();

        // How many options the target chooses (usually 1)
        public int chooseCount = 1;

        // TEMP until UI exists: which option(s) to auto-pick
        // Example: 0 means "always pick options[0]"
        public List<int> defaultPickIndices = new() { 0 };

        public override void Execute(CombatState state, EntityInstance source, TargetResult targets)
        {
            if (options == null || options.Count == 0) return;

            int n = Mathf.Clamp(chooseCount, 1, options.Count);

            var chosen = BuildDefaultChoice(n);

            // Apply chosen option(s) to EACH target
            var list = targets.GetEntities(targetLabel);
            for (int i = 0; i < list.Count; i++)
            {
                var singleTarget = list[i];

                // Build a TargetResult that contains only this one target under the same label,
                // so options apply only to them (important!)
                var single = new TargetResult();
                single.SetEntities(targetLabel, new List<EntityInstance> { singleTarget });

                for (int k = 0; k < chosen.Count; k++)
                {
                    int idx = chosen[k];
                    if (idx < 0 || idx >= options.Count) continue;
                    if (options[idx] == null) continue;

                    options[idx].Execute(state, source, single);
                }
            }
        }

        private List<int> BuildDefaultChoice(int n)
        {
            // Ensure we return exactly n unique indices
            var result = new List<int>();

            for (int i = 0; i < defaultPickIndices.Count && result.Count < n; i++)
            {
                int idx = defaultPickIndices[i];
                if (idx < 0) continue;
                if (result.Contains(idx)) continue;
                result.Add(idx);
            }

            // If not enough defaults provided, fill from 0 upward
            for (int idx = 0; result.Count < n && idx < options.Count; idx++)
            {
                if (!result.Contains(idx))
                    result.Add(idx);
            }

            return result;
        }
    }
}
