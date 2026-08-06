using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Combat.Conditions;
using Combat.Core;
using Combat.Data.Effects;
using Combat.Data.Enemies;
using Combat.Enemies;
using Combat.Entities;
using Combat.Resolution;
using UI.Combat;

namespace Combat.Runner
{
    // Runs the enemy phase as a paced sequence: for each enemy, announce its
    // draws, then reveal and resolve each drawn card one at a time.
    public class CombatEnemyTurnController
    {
        private readonly CombatRunner runner;
        private EnemyTurnPresenter presenter;

        public CombatEnemyTurnController(CombatRunner runner)
        {
            this.runner = runner;
        }

        public bool ShouldStartEnemySequence()
        {
            return runner.State.loop.phase == CombatPhase.Enemies
                && !runner.IsEnemyTurnSequenceRunning;
        }

        public IEnumerator RunEnemyPhaseSequence()
        {
            if (runner.State == null || runner.State.IsCombatOver)
                yield break;

            runner.IsEnemyTurnSequenceRunning = true;
            runner.IsCardFlowLocked = true;

            EnsurePresenter();

            CombatState state = runner.State;

            EnemyTurnExecutor.ResetEnemyBlockAtTurnStart(state);

            List<EnemyInstance> enemies = EnemyTurnExecutor.GetActingEnemies(state);

            Debug.Log($"ENEMY PHASE SEQUENCE BEGIN | acting enemies: {enemies.Count}");

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyInstance enemy = enemies[i];

                // May have been knocked out earlier in this same phase.
                if (state.conditions.Has(enemy, ConditionIds.Unconscious))
                    continue;

                List<EnemyCardType> drawn = state.enemyDeck.Draw(EnemyTurnExecutor.GetDrawCount(enemy));

                if (presenter != null)
                    presenter.SetActingEnemy(enemy);

                if (presenter != null)
                {
                    string plural = drawn.Count == 1 ? "" : "s";
                    yield return presenter.ShowBanner(DisplayName(enemy), $"Draws {drawn.Count} card{plural}");
                }

                for (int c = 0; c < drawn.Count; c++)
                {
                    EnemyAbilitySO ability = EnemyTurnExecutor.GetAbility(enemy.archetype, drawn[c]);
                    if (ability == null)
                        continue;

                    Debug.Log($"ENEMY CARD PLAY | {enemy.id} | {drawn[c]} | {ability.name}");

                    if (presenter != null)
                    {
                        yield return presenter.ShowCardPlay(
                            $"{DisplayName(enemy)} plays {AbilityTitle(ability, drawn[c])}",
                            DescribeAbility(ability)
                        );
                    }

                    EffectExecutor.ExecuteAbility(state, enemy, ability.targeting, ability.effects);

                    // Effects like TargetChoice hand a decision to the player;
                    // hold the sequence until every pending choice resolves.
                    while (TargetChoiceEffectSO.HasPendingRequests)
                        yield return null;

                    if (state.IsCombatOver)
                        break;
                }

                if (presenter != null)
                    presenter.ClearActingEnemy();

                if (state.IsCombatOver)
                    break;
            }

            if (presenter != null)
                presenter.ClearActingEnemy();

            state.loop.FinishEnemyPhase(state);

            runner.IsEnemyTurnSequenceRunning = false;

            if (!runner.IsDrawQueueRunning && !runner.IsStartRoundSequenceRunning)
                runner.IsCardFlowLocked = false;

            Debug.Log("ENEMY PHASE SEQUENCE COMPLETE");
        }

        private void EnsurePresenter()
        {
            if (presenter != null)
                return;

            presenter = Object.FindFirstObjectByType<EnemyTurnPresenter>();

            if (presenter == null)
                presenter = new GameObject("EnemyTurnPresenter").AddComponent<EnemyTurnPresenter>();
        }

        private static string DisplayName(EnemyInstance enemy)
        {
            if (enemy.archetype != null && !string.IsNullOrWhiteSpace(enemy.archetype.displayName))
                return enemy.archetype.displayName;

            return enemy.id;
        }

        private static string AbilityTitle(EnemyAbilitySO ability, EnemyCardType cardType)
        {
            return !string.IsNullOrWhiteSpace(ability.displayName) ? ability.displayName : cardType.ToString();
        }

        private static string DescribeAbility(EnemyAbilitySO ability)
        {
            return EffectDescriber.DescribeList(ability.effects);
        }
    }
}
