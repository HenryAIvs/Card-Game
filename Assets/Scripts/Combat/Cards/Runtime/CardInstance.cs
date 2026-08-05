using Combat.Data.Cards;

namespace Combat.Cards
{
    // Runtime instance of a card in deck/hand/discard.
    public class CardInstance
    {
        public HeroCardSO card;

        public CardInstance(HeroCardSO card)
        {
            this.card = card;
        }
    }
}
