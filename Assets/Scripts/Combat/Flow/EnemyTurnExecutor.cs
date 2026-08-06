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
        public static List<EnemyInstance> GetActingEnemies(CombatState state)
        {
            return state.lane.entities
                .Where(e =>
                    e.faction == Faction.Villain &&
                    e.disposition == Disposition.Hostile &&
                    !state.conditions.Has(e, ConditionIds.Unconscious))
                .OrderByDescending(e => e.finesse)
                .OfType<EnemyInstance>()
                .ToList();
        }

        public static int GetDrawCount(EnemyInstance enemy)
        {
            return 1 + Math.Max(0, enemy.finesse);
        }

        // Enemy block lasts until the enemies act again: at the start of
        // their turn any leftover block clears and their passive brawn
        // block comes back fresh.
        public static void ResetEnemyBlockAtTurnStart(CombatState state)
        {
            foreach (var e in state.lane.entities)
            {
                if (e.faction == Faction.Villain)
                    state.statuses.Set(e, Statuses.StatusId.Block, Math.Max(0, e.brawn));
            }
        }

        // Instant, non-animated version. Normal play goes through
        // CombatRunner's enemy turn sequence instead.
        public static bool ExecuteEnemyPhase(CombatState state, EnemyDeckState enemyDeck)
        {
            ResetEnemyBlockAtTurnStart(state);

            foreach (var enemy in GetActingEnemies(state))
            {
                var drawn = enemyDeck.Draw(GetDrawCount(enemy));

                foreach (var cardType in drawn)
                {
                    var ability = GetAbility(enemy.archetype, cardType);
                    if (ability == null) continue;

                    EffectExecutor.ExecuteAbility(state, enemy, ability.targeting, ability.effects);

                    if (state.IsCombatOver)
                        return true;
                }
            }

            return false;
        }

        public static EnemyAbilitySO GetAbility(EnemyArchetypeSO arch, EnemyCardType type)
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
