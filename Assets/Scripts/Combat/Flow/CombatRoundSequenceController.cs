using System.Collections;
using System.Linq;
using UnityEngine;
using Combat.Core;
using Combat.Entities;
using Combat.Conditions;
using Combat.Cards;

namespace Combat.Runner
{
    public class CombatRoundSequenceController
    {
        private readonly CombatRunner runner;
        private readonly CombatDrawQueueController drawQueueController;

        public CombatRoundSequenceController(
            CombatRunner runner,
            CombatDrawQueueController drawQueueController
        )
        {
            this.runner = runner;
            this.drawQueueController = drawQueueController;
        }

        public bool ShouldStartRoundSequence()
        {
            return runner.State.loop.phase == CombatPhase.StartRound
                && !runner.IsStartRoundSequenceRunning;
        }

        public IEnumerator RunStartRoundSequence()
        {
            if (runner.State == null || runner.State.IsCombatOver)
                yield break;

            BeginStartRoundSequence();
            runner.State.loop.PrepareStartRound(runner.State);

            // Important:
            // give the UI one rendered frame to settle before the first draw animation.
            yield return null;

            yield return RunRoundStartDraws();

            runner.State.loop.FinishStartRound(runner.State);
            EndStartRoundSequence();
            DebugLogPostStartRoundState();
        }

        private void BeginStartRoundSequence()
        {
            runner.IsStartRoundSequenceRunning = true;
            runner.IsCardFlowLockedInternal = true;
        }

        private void EndStartRoundSequence()
        {
            runner.IsStartRoundSequenceRunning = false;

            if (!runner.IsDrawQueueRunning)
                runner.IsCardFlowLockedInternal = false;
        }

        private IEnumerator RunRoundStartDraws()
        {
            drawQueueController.EnsureHandUI();

            Debug.Log("START ROUND DRAWS BEGIN");

            for (int i = 0; i < runner.State.heroes.Count; i++)
            {
                HeroInstance hero = runner.State.heroes[i];
                if (!CanHeroDrawAtRoundStart(hero))
                    continue;

                int drawCount = hero.GetRoundDrawCount(runner.State);

                Debug.Log(
                    $"ROUND DRAW HERO | hero={hero.id} | heroIndex={i} | drawCount={drawCount} | handBefore={hero.deck.hand.Count}"
                );

                for (int drawIndex = 0; drawIndex < drawCount; drawIndex++)
                {
                    CardInstance drawnCard = hero.deck.DrawOneToHand();
                    if (drawnCard == null)
                        break;

                    Debug.Log(
                        $"ROUND DRAW CARD | hero={hero.id} | drawIndex={drawIndex} | card={drawnCard.card.displayName} | handAfter={hero.deck.hand.Count}"
                    );

                    runner.ObservedHandCounts[hero] = hero.deck.hand.Count;

                    Debug.Log(
                        $"ROUND DRAW ANIMATE REQUEST | hero={hero.id} | card={drawnCard.card.displayName}"
                    );

                    yield return drawQueueController.AnimateOrRefreshDrawnCard(hero, drawnCard);
                }
            }
        }

        private bool CanHeroDrawAtRoundStart(HeroInstance hero)
        {
            if (hero == null)
                return false;

            return !runner.State.conditions.Has(hero, ConditionIds.Unconscious);
        }

        private void DebugLogPostStartRoundState()
        {
            Debug.Log($"AFTER StartRound Sequence: Phase {runner.State.loop.phase}");

            for (int i = 0; i < runner.State.heroes.Count; i++)
            {
                HeroInstance hero = runner.State.heroes[i];
                Debug.Log($"{hero.id} energy: R{hero.energyCurrent.red} Y{hero.energyCurrent.yellow} B{hero.energyCurrent.blue}");
                Debug.Log($"{hero.id} hand: {string.Join(",", hero.deck.hand.Select(c => c.card.displayName))}");
            }
        }
    }
}