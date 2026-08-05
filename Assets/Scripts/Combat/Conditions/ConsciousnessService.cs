using Combat.Core;
using Combat.Entities;

namespace Combat.Conditions
{
    public static class ConsciousnessService
    {
        /// <summary>
        /// The ONLY place where an entity becomes unconscious.
        /// Triggers end-of-combat check immediately.
        /// Returns true if the entity was newly made unconscious.
        /// </summary>
        public static bool MakeUnconscious(
            CombatState state,
            EntityInstance entity,
            ConditionDuration duration = ConditionDuration.Temporary
        )
        {
            if (state.conditions.Has(entity, ConditionIds.Unconscious))
                return false;

            state.conditions.Apply(entity, ConditionIds.Unconscious, duration);

            // End-combat check happens ONLY here.
            state.loop.TryEndCombat(state);

            return true;
        }
    }
}
