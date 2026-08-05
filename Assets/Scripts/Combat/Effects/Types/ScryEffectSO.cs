using System.Collections.Generic;
using UnityEngine;
using Combat.Core;
using Combat.Entities;
using Combat.Cards;
using Combat.Enemies;

namespace Combat.Data.Effects
{
    public enum ScryReorderMode
    {
        NoChange,
        Reverse,
        SortByName
    }

    [CreateAssetMenu(menuName = "Combat/Effects/Scry", fileName = "Effect_Scry")]
    public class ScryEffectSO : EffectSO
    {
        public override string Keyword => "Scry";

        public int amount = 1;              // Scry X
        public string targetLabel = "T1";   // who we are "scrying"

        // TEMP until UI exists
        public ScryReorderMode reorderMode = ScryReorderMode.NoChange;

        public override void Execute(CombatState state, EntityInstance source, Targeting.TargetResult targets)
        {
            var list = targets.GetEntities(targetLabel);

            for (int i = 0; i < list.Count; i++)
            {
                var t = list[i];
                int x = Mathf.Max(0, amount);

                // If target is a Villain, scry the shared enemy deck
                if (t.faction == Faction.Villain)
                {
                    var top = state.enemyDeck.PeekTop(x);
                    if (top.Count <= 1) continue;

                    var reordered = ReorderEnemy(top);
                    state.enemyDeck.SetTopOrder(reordered);

                    Debug.Log($"[Scry] Scried ENEMY deck top {top.Count} via {reorderMode}.");
                    continue;
                }

                // If target is a Hero, scry their personal deck
                if (t is HeroInstance hero)
                {
                    var top = hero.deck.PeekTop(x);
                    if (top.Count <= 1) continue;

                    var reordered = ReorderHero(top);
                    hero.deck.SetTopOrder(reordered);

                    Debug.Log($"[Scry] {hero.id} scried top {top.Count} via {reorderMode}.");
                }
            }
        }

        private List<CardInstance> ReorderHero(List<CardInstance> top)
        {
            var result = new List<CardInstance>(top);

            switch (reorderMode)
            {
                case ScryReorderMode.Reverse:
                    result.Reverse();
                    return result;

                case ScryReorderMode.SortByName:
                    result.Sort((a, b) =>
                    {
                        string an = a?.card != null ? a.card.displayName : "";
                        string bn = b?.card != null ? b.card.displayName : "";
                        return string.CompareOrdinal(an, bn);
                    });
                    return result;

                case ScryReorderMode.NoChange:
                default:
                    return result;
            }
        }

        private List<EnemyCardType> ReorderEnemy(List<EnemyCardType> top)
        {
            var result = new List<EnemyCardType>(top);

            switch (reorderMode)
            {
                case ScryReorderMode.Reverse:
                    result.Reverse();
                    return result;

                case ScryReorderMode.SortByName:
                    // Sort by enum name (Attack, Defense, Special1, Special2)
                    result.Sort((a, b) => string.CompareOrdinal(a.ToString(), b.ToString()));
                    return result;

                case ScryReorderMode.NoChange:
                default:
                    return result;
            }
        }
    }
}
