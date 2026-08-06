using System.Collections.Generic;
using Combat.Data.Cards;
using Combat.Data.Effects;
using Combat.Targeting;
using Combat.Entities;

namespace Combat.Cards
{
    // Stores what to execute later (after enemies).
    public class SlowCardPlay
    {
        public HeroInstance source;
        public string cardName;

        // The played card, kept so the slow resolve phase can show it.
        public HeroCardSO card;

        public List<TargetStep> targeting;
        public List<EffectSO> effects;

        public SlowCardPlay(
            HeroInstance source,
            HeroCardSO card,
            List<TargetStep> targeting,
            List<EffectSO> effects
        )
        {
            this.source = source;
            this.card = card;
            this.cardName = card != null ? card.displayName : "";
            this.targeting = targeting;
            this.effects = effects;
        }
    }
}
