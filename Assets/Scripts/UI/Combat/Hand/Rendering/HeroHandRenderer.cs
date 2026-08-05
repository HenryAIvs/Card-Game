using System.Collections.Generic;
using UnityEngine;
using Combat.Cards;

namespace UI.Combat
{
    public class HeroHandRenderer
    {
        private readonly HeroHandUI owner;
        private readonly HeroHandCardLookup cardLookup;

        public HeroHandRenderer(HeroHandUI owner, HeroHandCardLookup cardLookup)
        {
            this.owner = owner;
            this.cardLookup = cardLookup;
        }

        public void RefreshHand(bool immediate = true)
        {
            Transform cardContainer = owner.GetCardContainer();
            HandCardUI cardPrefab = owner.GetCardPrefab();

            if (cardContainer == null || cardPrefab == null || owner.BoundHero == null)
                return;

            Dictionary<CardInstance, HandCardUI> existingCards =
                cardLookup.BuildExistingCardMap(cardContainer);

            SyncCardsToHand(existingCards, cardContainer, cardPrefab);
            RemoveStaleCards(existingCards);
            RefreshFanLayout(cardContainer, immediate);
        }

        public void RefreshPlayableStates()
        {
            Transform cardContainer = owner.GetCardContainer();

            if (cardContainer == null || owner.BoundHero == null)
                return;

            for (int i = 0; i < cardContainer.childCount; i++)
            {
                HandCardUI cardUI = cardContainer.GetChild(i).GetComponent<HandCardUI>();
                if (cardUI == null)
                    continue;

                CardInstance card = cardUI.BoundCard;
                bool isPlayable = owner.CanCurrentlyPlayCard(card);
                cardUI.SetPlayableState(isPlayable);
            }
        }

        private void SyncCardsToHand(
            Dictionary<CardInstance, HandCardUI> existingCards,
            Transform cardContainer,
            HandCardUI cardPrefab
        )
        {
            for (int displayIndex = 0; displayIndex < owner.BoundHero.deck.hand.Count; displayIndex++)
            {
                CardInstance card = GetDisplayedCardAtIndex(displayIndex);
                if (card == null)
                    continue;

                HandCardUI cardUI = GetOrCreateCardUI(card, existingCards, cardContainer, cardPrefab);
                ApplyDisplayState(cardUI, card, displayIndex);

                existingCards.Remove(card);
            }
        }

        private CardInstance GetDisplayedCardAtIndex(int displayIndex)
        {
            int handIndex = owner.BoundHero.deck.hand.Count - 1 - displayIndex;

            if (handIndex < 0 || handIndex >= owner.BoundHero.deck.hand.Count)
                return null;

            return owner.BoundHero.deck.hand[handIndex];
        }

        private HandCardUI GetOrCreateCardUI(
            CardInstance card,
            Dictionary<CardInstance, HandCardUI> existingCards,
            Transform cardContainer,
            HandCardUI cardPrefab
        )
        {
            if (existingCards.TryGetValue(card, out HandCardUI existingCardUI) && existingCardUI != null)
                return existingCardUI;

            HandCardUI newCardUI = Object.Instantiate(cardPrefab, cardContainer);
            newCardUI.Bind(card);
            return newCardUI;
        }

        private void ApplyDisplayState(HandCardUI cardUI, CardInstance card, int displayIndex)
        {
            if (cardUI == null)
                return;

            HandCardVisualState visualState = cardUI.GetComponent<HandCardVisualState>();
            if (visualState != null)
                visualState.SetOriginalSiblingIndex(displayIndex);

            cardUI.transform.SetSiblingIndex(displayIndex);
            cardUI.SetPlayableState(owner.CanCurrentlyPlayCard(card));
        }

        private void RemoveStaleCards(Dictionary<CardInstance, HandCardUI> remainingCards)
        {
            foreach (var pair in remainingCards)
            {
                if (pair.Value == null)
                    continue;

                Object.Destroy(pair.Value.gameObject);
            }
        }

        private void RefreshFanLayout(Transform cardContainer, bool immediate)
        {
            HandFanLayout fanLayout = cardContainer.GetComponent<HandFanLayout>();
            if (fanLayout == null)
                return;

            if (immediate)
                fanLayout.ForceRefresh();
        }
    }
}