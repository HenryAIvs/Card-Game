using System.Collections;
using UnityEngine;
using Combat.Entities;
using Combat.Cards;
using UI.Combat;

namespace Combat.Runner
{
    public class CombatDrawQueueController
    {
        private readonly CombatRunner runner;

        public CombatDrawQueueController(CombatRunner runner)
        {
            this.runner = runner;
        }

        public void EnsureHandUI()
        {
            if (runner.CachedHandUI == null)
                runner.CachedHandUI = Object.FindFirstObjectByType<HeroHandUI>();
        }

        public void DetectMidTurnDraws()
        {
            if (runner.State == null || runner.State.heroes == null)
                return;

            bool queuedAny = false;

            for (int i = 0; i < runner.State.heroes.Count; i++)
            {
                HeroInstance hero = runner.State.heroes[i];
                if (hero == null)
                    continue;

                int currentCount = hero.deck.hand.Count;
                int knownCount = runner.ObservedHandCounts.TryGetValue(hero, out int value) ? value : 0;

                if (currentCount > knownCount)
                {
                    QueueNewlyDrawnCards(hero, knownCount, currentCount);
                    runner.ObservedHandCounts[hero] = currentCount;
                    queuedAny = true;
                }
                else if (currentCount < knownCount)
                {
                    runner.ObservedHandCounts[hero] = currentCount;
                }
            }

            if (queuedAny && !runner.IsDrawQueueRunning && !runner.IsStartRoundSequenceRunning)
                runner.StartCoroutine(ProcessDrawQueue());
        }

        private void QueueNewlyDrawnCards(HeroInstance hero, int knownCount, int currentCount)
        {
            int added = currentCount - knownCount;

            for (int handIndex = currentCount - added; handIndex < currentCount; handIndex++)
            {
                if (handIndex < 0 || handIndex >= hero.deck.hand.Count)
                    continue;

                CardInstance drawnCard = hero.deck.hand[handIndex];
                runner.DrawQueue.Enqueue(new DrawRequest(hero, drawnCard));
            }
        }

        public IEnumerator ProcessDrawQueue()
        {
            if (runner.IsDrawQueueRunning)
                yield break;

            runner.IsDrawQueueRunning = true;
            runner.IsCardFlowLockedInternal = true;

            EnsureHandUI();

            while (runner.DrawQueue.Count > 0)
            {
                DrawRequest request = runner.DrawQueue.Dequeue();
                yield return AnimateOrRefreshDrawnCard(request.hero, request.card);
            }

            runner.IsDrawQueueRunning = false;

            if (!runner.IsStartRoundSequenceRunning)
                runner.IsCardFlowLockedInternal = false;
        }

        public IEnumerator AnimateOrRefreshDrawnCard(HeroInstance hero, CardInstance drawnCard)
        {
            if (hero == null || drawnCard == null)
                yield break;

            EnsureHandUI();

            if (runner.CachedHandUI == null)
                yield break;

            SyncHandUIBindingWithoutRefresh(hero);

            if (runner.CachedHandUI.BoundHero == hero)
            {
                yield return runner.CachedHandUI.PlayDrawAnimation(drawnCard);
            }
            else
            {
                runner.CachedHandUI.RefreshHand(true);
                yield return null;
            }
        }

        private void SyncHandUIBindingWithoutRefresh(HeroInstance hero)
        {
            if (runner.CachedHandUI == null || runner.State == null || runner.State.heroes == null)
                return;

            int heroIndex = runner.State.heroes.IndexOf(hero);
            if (heroIndex < 0)
                return;

            if (runner.CachedHandUI.SelectedHeroIndex != heroIndex || runner.CachedHandUI.BoundHero != hero)
                runner.CachedHandUI.SetSelectedHeroIndex(heroIndex, forceRefresh: false);
        }
    }
}