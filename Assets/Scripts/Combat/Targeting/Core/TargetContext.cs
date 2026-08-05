using System.Collections.Generic;
using Combat.Core;
using Combat.Entities;
using Combat.Data.Effects;

namespace Combat.Targeting
{
    public class TargetContext
    {
        public CombatState state;
        public EntityInstance source;
        public TargetStep step;
        public TargetResult result;
        public List<EffectSO> effects;

        public TargetContext(
            CombatState state,
            EntityInstance source,
            TargetStep step,
            TargetResult result,
            List<EffectSO> effects
        )
        {
            this.state = state;
            this.source = source;
            this.step = step;
            this.result = result;
            this.effects = effects;
        }
    }
}