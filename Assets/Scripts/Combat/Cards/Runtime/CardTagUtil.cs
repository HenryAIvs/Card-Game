using Combat.Data.Cards;

namespace Combat.Cards
{
    public static class CardTagUtil
    {
        public static bool HasTag(HeroCardSO card, CardTag tag)
        {
            return card != null && card.tags != null && card.tags.Contains(tag);
        }
    }
}
