using UnityEngine;
using Combat.Core;
using Combat.Entities;
using Combat.Statuses;
using Combat.Targeting;

namespace Combat.Data.Effects
{
    [CreateAssetMenu(menuName = "Combat/Effects/Change Status", fileName = "Effect_ChangeStatus")]
    public class ChangeStatusEffectSO : EffectSO
    {
        public override string Keyword => "ChangeStatus";

        public StatusId status = StatusId.Block;
        public int amount = 1;              // can be negative to reduce
        public bool clampToZero = true;
        public string targetLabel = "T1";

        public override void Execute(CombatState state, EntityInstance source, TargetResult targets)
        {
            var list = targets.GetEntities(targetLabel);
            for (int i = 0; i < list.Count; i++)
            {
                state.statuses.Add(list[i], status, amount, clampToZero);
            }
        }
    }
}
