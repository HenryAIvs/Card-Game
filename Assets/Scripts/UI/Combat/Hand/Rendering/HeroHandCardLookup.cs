using System.Collections.Generic;
using UnityEngine;
using Combat.Cards;

namespace UI.Combat
{
    public class HeroHandCardLookup
    {
        private readonly HeroHandUI owner;

        public HeroHandCardLookup(HeroHandUI owner)
        {
            this.owner = owner;
        }

        public HandCardUI FindCardUI(CardInstance targetCard)
        {
            Transform cardContainer = owner.GetCardContainer();
            if (cardContainer == null || targetCard == null)
                return null;

            for (int i = 0; i < cardContainer.childCount; i++)
            {
                HandCardUI cardUI = cardContainer.GetChild(i).GetComponent<HandCardUI>();
                if (cardUI == null)
                    continue;

                if (ReferenceEquals(cardUI.BoundCard, targetCard))
                    return cardUI;
            }

            return null;
        }

        public Dictionary<CardInstance, HandCardUI> BuildExistingCardMap(Transform cardContainer)
        {
            Dictionary<CardInstance, HandCardUI> result = new Dictionary<CardInstance, HandCardUI>();

            if (cardContainer == null)
                return result;

            for (int i = 0; i < cardContainer.childCount; i++)
            {
                HandCardUI cardUI = cardContainer.GetChild(i).GetComponent<HandCardUI>();
                if (cardUI == null || cardUI.BoundCard == null)
                    continue;

                if (!result.ContainsKey(cardUI.BoundCard))
                    result.Add(cardUI.BoundCard, cardUI);
            }

            return result;
        }
    }
}