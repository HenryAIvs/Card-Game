using System.Collections.Generic;
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

        public List<TargetStep> targeting;
        public List<EffectSO> effects;

        public SlowCardPlay(HeroInstance source, string cardName, List<TargetStep> targeting, List<EffectSO> effects)
        {
            this.source = source;
            this.cardName = cardName;
            this.targeting = targeting;
            this.effects = effects;
        }
    }
}
