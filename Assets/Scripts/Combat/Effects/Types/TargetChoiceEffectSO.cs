using System;
using System.Collections.Generic;
using UnityEngine;
using Combat.Core;
using Combat.Entities;
using Combat.Targeting;

namespace Combat.Data.Effects
{
    // A pending "target chooses which option affects them" decision. The UI
    // resolves it via ResolvePick; the enemy turn sequence waits while any
    // request is pending so effects keep resolving in order.
    public class TargetChoiceRequest
    {
        public readonly TargetChoiceEffectSO effect;
        public readonly CombatState state;
        public readonly EntityInstance source;
        public readonly EntityInstance target;

        public bool IsResolved { get; private set; }

        public TargetChoiceRequest(
            TargetChoiceEffectSO effect,
            CombatState state,
            EntityInstance source,
            EntityInstance target
        )
        {
            this.effect = effect;
            this.state = state;
            this.source = source;
            this.target = target;
        }

        public void ResolvePick(int optionIndex)
        {
            if (IsResolved)
                return;

            IsResolved = true;
            TargetChoiceEffectSO.CompleteRequest(this, optionIndex);
        }
    }

    [CreateAssetMenu(menuName = "Combat/Effects/Target Choice", fileName = "Effect_TargetChoice")]
    public class TargetChoiceEffectSO : EffectSO
    {
        public override string Keyword => "TargetChoice";

        // Raised when a hero must pick an option. With no subscriber, the
        // default pick applies immediately instead.
        public static event Action<TargetChoiceRequest> OnChoiceRequested;

        private static readonly List<TargetChoiceRequest> pendingRequests = new();

        public static bool HasPendingRequests => pendingRequests.Count > 0;

        public static void ClearPendingRequests()
        {
            pendingRequests.Clear();
        }

        // Which targets are affected by this choice (usually T1)
        public string targetLabel = "T1";

        // Effects the TARGET chooses between
        public List<EffectSO> options = new();

        // How many options the target chooses (usually 1)
        public int chooseCount = 1;

        // Fallback picks when no UI is subscribed, or for non-hero targets.
        public List<int> defaultPickIndices = new() { 0 };

        public override void Execute(CombatState state, EntityInstance source, TargetResult targets)
        {
            if (options == null || options.Count == 0) return;

            int n = Mathf.Clamp(chooseCount, 1, options.Count);

            var list = targets.GetEntities(targetLabel);
            for (int i = 0; i < list.Count; i++)
            {
                var singleTarget = list[i];

                // Heroes decide for themselves through the choice UI.
                if (singleTarget is HeroInstance && OnChoiceRequested != null)
                {
                    var request = new TargetChoiceRequest(this, state, source, singleTarget);
                    pendingRequests.Add(request);

                    Debug.Log($"TARGET CHOICE REQUEST | target={singleTarget.id} | options={options.Count}");
                    OnChoiceRequested.Invoke(request);
                    continue;
                }

                ApplyOptions(state, source, singleTarget, BuildDefaultChoice(n));
            }
        }

        internal static void CompleteRequest(TargetChoiceRequest request, int pickedIndex)
        {
            pendingRequests.Remove(request);

            TargetChoiceEffectSO effect = request.effect;
            int n = Mathf.Clamp(effect.chooseCount, 1, effect.options.Count);

            var picks = new List<int>();

            if (pickedIndex >= 0 && pickedIndex < effect.options.Count)
                picks.Add(pickedIndex);

            // chooseCount above 1 tops up from the defaults.
            for (int i = 0; i < effect.defaultPickIndices.Count && picks.Count < n; i++)
            {
                int idx = effect.defaultPickIndices[i];
                if (idx >= 0 && idx < effect.options.Count && !picks.Contains(idx))
                    picks.Add(idx);
            }

            for (int idx = 0; picks.Count < n && idx < effect.options.Count; idx++)
            {
                if (!picks.Contains(idx))
                    picks.Add(idx);
            }

            Debug.Log($"TARGET CHOICE RESOLVED | target={request.target.id} | picked={pickedIndex}");
            effect.ApplyOptions(request.state, request.source, request.target, picks);
        }

        // Applies the given option indices to a single target only, so each
        // target's choice affects nobody else.
        private void ApplyOptions(
            CombatState state,
            EntityInstance source,
            EntityInstance singleTarget,
            List<int> indices
        )
        {
            var single = new TargetResult();
            single.SetEntities(targetLabel, new List<EntityInstance> { singleTarget });

            for (int k = 0; k < indices.Count; k++)
            {
                int idx = indices[k];
                if (idx < 0 || idx >= options.Count) continue;
                if (options[idx] == null) continue;

                options[idx].Execute(state, source, single);
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
