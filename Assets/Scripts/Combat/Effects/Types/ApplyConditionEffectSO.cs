using UnityEngine;
using Combat.Conditions;
using Combat.Core;
using Combat.Entities;
using Combat.Targeting;

namespace Combat.Data.Effects
{
    [CreateAssetMenu(menuName = "Combat/Effects/Apply Condition", fileName = "Effect_ApplyCondition")]
    public class ApplyConditionEffectSO : EffectSO
    {
        public override string Keyword => "ApplyCondition";

        public string conditionId = ConditionIds.Exhaustion;
        public ConditionDuration duration = ConditionDuration.Temporary;

        public string targetLabel = "T1";

        public override void Execute(CombatState state, EntityInstance source, TargetResult targets)
        {
            var list = targets.GetEntities(targetLabel);
            for (int i = 0; i < list.Count; i++)
            {
                state.conditions.Apply(list[i], conditionId, duration);
            }
        }
    }
}
