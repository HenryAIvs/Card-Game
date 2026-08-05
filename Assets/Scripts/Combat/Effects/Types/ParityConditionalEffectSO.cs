using System.Collections.Generic;
using UnityEngine;
using Combat.Core;
using Combat.Entities;
using Combat.Targeting;
using Combat.Resolution.Types;

namespace Combat.Data.Effects
{
    [CreateAssetMenu(menuName = "Combat/Effects/Parity Conditional", fileName = "Effect_Parity")]
    public class ParityConditionalEffectSO : EffectSO
    {
        public override string Keyword => parity == ParityType.Left ? "Left" : "Right";

        public ParityType parity;
        public List<EffectSO> effects = new();

        public override void Execute(CombatState state, EntityInstance source, TargetResult targets)
        {
            if (source is not HeroInstance hero)
                return;

            int handSize = hero.deck.hand.Count;
            bool isOdd = (handSize % 2) == 1;

            bool conditionMet =
                (parity == ParityType.Left && isOdd) ||
                (parity == ParityType.Right && !isOdd);

            Debug.Log(
                $"PARITY CHECK | Card parity: {parity} | Hero: {hero.id} | Hand size: {handSize} | " +
                $"Odd: {isOdd} | Condition met: {conditionMet}"
            );

            if (!conditionMet)
                return;

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] == null) continue;

                Debug.Log($"PARITY EXECUTE | {parity} triggered effect: {effects[i].name}");
                effects[i].Execute(state, source, targets);
            }
        }
    }
}