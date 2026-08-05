using System;
using System.Collections.Generic;
using System.Linq;
using Combat.Conditions;
using Combat.Core;
using Combat.Data.Enemies;
using Combat.Entities;
using Combat.Enemies;
using Combat.Resolution;

namespace Combat.Enemies
{
    public static class EnemyTurnExecutor
    {
        public static bool ExecuteEnemyPhase(CombatState state, EnemyDeckState enemyDeck)
        {
            var enemies = state.lane.entities
                .Where(e =>
                    e.faction == Faction.Villain &&
                    e.disposition == Disposition.Hostile &&
                    !state.conditions.Has(e, ConditionIds.Unconscious))
                .OrderByDescending(e => e.finesse)
                .ToList();

            foreach (var enemyEntity in enemies)
            {
                int drawCount = 1 + Math.Max(0, enemyEntity.finesse);
                var drawn = enemyDeck.Draw(drawCount);

                if (enemyEntity is not EnemyInstance enemy) continue;

                foreach (var cardType in drawn)
                {
                    var ability = GetAbility(enemy.archetype, cardType);
                    if (ability == null) continue;

                    EffectExecutor.ExecuteAbility(state, enemyEntity, ability.targeting, ability.effects);

                    if (state.IsCombatOver)
                        return true;

                }
            }

            return false;
        }

        private static EnemyAbilitySO GetAbility(EnemyArchetypeSO arch, EnemyCardType type)
        {
            return type switch
            {
                EnemyCardType.Attack => arch.attack,
                EnemyCardType.Defense => arch.defense,
                EnemyCardType.Special1 => arch.special1,
                EnemyCardType.Special2 => arch.special2,
                _ => null
            };
        }
    }
}
