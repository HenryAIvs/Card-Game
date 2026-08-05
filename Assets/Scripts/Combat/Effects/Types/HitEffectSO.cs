using UnityEngine;
using Combat.Core;
using Combat.Entities;
using Combat.Resolution;
using Combat.Targeting;

namespace Combat.Data.Effects
{
    [CreateAssetMenu(menuName = "Combat/Effects/Hit", fileName = "Effect_Hit")]
    public class HitEffectSO : EffectSO
    {
        public override string Keyword => "Hit";

        public int amount = 1;
        public string targetLabel = "T1";

        public override void Execute(CombatState state, EntityInstance source, TargetResult targets)
        {
            var list = targets.GetEntities(targetLabel);

            Debug.Log(
                $"HIT EFFECT | Source: {source.id} | Amount: {amount} | " +
                $"Target label: {targetLabel} | Targets found: {list.Count}"
            );

            for (int i = 0; i < list.Count; i++)
            {
                Debug.Log($"HIT TARGET | {list[i].id}");
                DamageResolver.ApplyHit(state, source, list[i], amount);
            }
        }
    }
}