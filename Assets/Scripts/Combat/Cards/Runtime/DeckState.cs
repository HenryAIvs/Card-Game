using System;
using System.Collections.Generic;

namespace Combat.Cards
{
    public class DeckState
    {
        public readonly List<CardInstance> drawPile = new();
        public readonly List<CardInstance> discardPile = new();
        public readonly List<CardInstance> hand = new();

        private readonly Random rng = new();

        public void ShuffleDrawPile()
        {
            for (int i = drawPile.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (drawPile[i], drawPile[j]) = (drawPile[j], drawPile[i]);
            }
        }

        public void RefillDrawFromDiscardIfNeeded()
        {
            if (drawPile.Count > 0) return;
            if (discardPile.Count == 0) return;

            drawPile.AddRange(discardPile);
            discardPile.Clear();
            ShuffleDrawPile();
        }

        public CardInstance DrawOneToHand()
        {
            RefillDrawFromDiscardIfNeeded();
            if (drawPile.Count == 0)
                return null;

            CardInstance c = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(c);
            return c;
        }

        public void DiscardFromHand(CardInstance card)
        {
            if (!hand.Remove(card)) return;
            discardPile.Add(card);
        }

        public List<CardInstance> PeekTop(int count)
        {
            int n = Math.Min(count, drawPile.Count);
            var result = new List<CardInstance>(n);

            for (int i = 0; i < n; i++)
                result.Add(drawPile[i]);

            return result;
        }

        // Replaces the top N cards with the provided order.
        // The provided list must be exactly N cards that are currently the top N.
        public void SetTopOrder(List<CardInstance> newOrder)
        {
            if (newOrder == null || newOrder.Count == 0) return;
            if (newOrder.Count > drawPile.Count) return;

            drawPile.RemoveRange(0, newOrder.Count);
            drawPile.InsertRange(0, newOrder);
        }
    }
}