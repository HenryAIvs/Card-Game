using System;
using System.Collections.Generic;

namespace Combat.Enemies
{
    public class EnemyDeckState
    {
        public readonly List<EnemyCardType> drawPile = new();
        public readonly List<EnemyCardType> discardPile = new();

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

        public List<EnemyCardType> Draw(int count)
        {
            var drawn = new List<EnemyCardType>();

            for (int i = 0; i < count; i++)
            {
                RefillDrawFromDiscardIfNeeded();
                if (drawPile.Count == 0) break;

                var c = drawPile[0];
                drawPile.RemoveAt(0);
                drawn.Add(c);
                discardPile.Add(c); 
            }

            return drawn;
        }

        public List<EnemyCardType> PeekTop(int count)
        {
            int n = Math.Min(count, drawPile.Count);
            var result = new List<EnemyCardType>(n);
            for (int i = 0; i < n; i++)
                result.Add(drawPile[i]);
            return result;
        }

        public void SetTopOrder(List<EnemyCardType> newOrder)
        {
            if (newOrder == null || newOrder.Count == 0) return;

            int n = newOrder.Count;
            if (drawPile.Count < n) return;

            // Remove current top N and replace with new order
            drawPile.RemoveRange(0, n);
            drawPile.InsertRange(0, newOrder);
        }

    }
}
