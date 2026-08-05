using Combat.Conditions;
using Combat.Core;
using Combat.Entities;
using Combat.Statuses;

namespace Combat.Resolution
{
    public static class DamageResolver
    {
        public static bool ApplyHit(CombatState state, EntityInstance attacker, EntityInstance target, int hitAmount)
        {
            if (hitAmount <= 0) return false;

            int remaining = hitAmount;

            // Ward blocks the entire hit instance
            int ward = state.statuses.Get(target, StatusId.Ward);
            if (ward > 0)
            {
                state.statuses.Set(target, StatusId.Ward, ward - 1);
                remaining = 0;
            }

            // Block
            if (remaining > 0)
            {
                int block = state.statuses.Get(target, StatusId.Block);
                if (block > 0)
                {
                    int used = remaining <= block ? remaining : block;
                    state.statuses.Set(target, StatusId.Block, block - used);
                    remaining -= used;
                }
            }

            // Armour
            if (remaining > 0)
            {
                int armour = state.statuses.Get(target, StatusId.Armour);
                if (armour > 0)
                {
                    int used = remaining <= armour ? remaining : armour;
                    state.statuses.Set(target, StatusId.Armour, armour - used);
                    remaining -= used;
                }
            }

            if (remaining <= 0) return false;

            // Damage got through
            if (target.faction == Faction.Hero)
            {
                return ConsciousnessService.MakeUnconscious(state, target);
            }

            if (target.faction == Faction.Villain)
            {
                target.currentHp -= remaining;
                if (target.currentHp <= 0)
                {
                    target.currentHp = 0;
                    return ConsciousnessService.MakeUnconscious(state, target);
                }
            }

            return false;
        }
    }
}
