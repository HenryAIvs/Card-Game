using System.Collections;
using UnityEngine;
using Combat.Conditions;
using Combat.Core;
using Combat.Cards;
using Combat.Data.Effects;
using Combat.Resolution;
using UI.Combat;

namespace Combat.Runner
{
    // Runs the slow resolve phase as a paced sequence: each queued slow card
    // is shown before its effects execute.
    public class CombatSlowResolveController
    {
        private readonly CombatRunner runner;
        private SlowCardPresenterUI presenter;

        public CombatSlowResolveController(CombatRunner runner)
        {
            this.runner = runner;
        }

        public bool ShouldStartSlowResolveSequence()
        {
            return runner.State.loop.phase == CombatPhase.SlowResolve
                && !runner.IsSlowResolveSequenceRunning;
        }

        public IEnumerator RunSlowResolveSequence()
        {
            if (runner.State == null || runner.State.IsCombatOver)
                yield break;

            runner.IsSlowResolveSequenceRunning = true;
            runner.IsCardFlowLocked = true;

            EnsurePresenter();

            CombatState state = runner.State;

            Debug.Log($"SLOW RESOLVE SEQUENCE BEGIN | queued plays: {state.slowQueue.Count}");

            for (int i = 0; i < state.slowQueue.Count; i++)
            {
                SlowCardPlay play = state.slowQueue[i];

                if (play == null || state.conditions.Has(play.source, ConditionIds.Unconscious))
                    continue;

                Debug.Log($"SLOW CARD RESOLVE | {play.source.id} | {play.cardName}");

                if (presenter != null)
                    yield return presenter.ShowSlowCard(play);

                EffectExecutor.ExecuteAbility(state, play.source, play.targeting, play.effects);

                // Slow effects can also hand a decision to the player.
                while (TargetChoiceEffectSO.HasPendingRequests)
                    yield return null;

                if (state.IsCombatOver)
                    break;
            }

            state.loop.FinishSlowResolve(state);

            runner.IsSlowResolveSequenceRunning = false;

            if (!runner.IsDrawQueueRunning && !runner.IsStartRoundSequenceRunning && !runner.IsEnemyTurnSequenceRunning)
                runner.IsCardFlowLocked = false;

            Debug.Log("SLOW RESOLVE SEQUENCE COMPLETE");
        }

        private void EnsurePresenter()
        {
            if (presenter != null)
                return;

            presenter = Object.FindFirstObjectByType<SlowCardPresenterUI>();

            if (presenter == null)
                presenter = new GameObject("SlowCardPresenterUI").AddComponent<SlowCardPresenterUI>();
        }
    }
}
