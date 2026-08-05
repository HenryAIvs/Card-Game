using System.Collections.Generic;
using Combat.Entities;

namespace Combat.Conditions
{
    public class ConditionSystem
    {
        public void Apply(EntityInstance entity, string id, ConditionDuration duration)
        {
            if (entity.conditions.TryGetValue(id, out var existing))
            {
                // Keep the longer duration
                existing.duration = MaxDuration(existing.duration, duration);
                return;
            }

            entity.conditions[id] = new ConditionInstance(id, duration);
        }

        public void Remove(EntityInstance entity, string id)
        {
            entity.conditions.Remove(id);
        }

        public bool Has(EntityInstance entity, string id)
        {
            return entity.conditions.ContainsKey(id);
        }

        public void EndRoundCleanup(IEnumerable<EntityInstance> allEntities)
        {
            foreach (var e in allEntities)
                RemoveByDuration(e, ConditionDuration.Fleeting);
        }

        public void EndCombatCleanup(IEnumerable<EntityInstance> allEntities)
        {
            foreach (var e in allEntities)
            {
                RemoveByDuration(e, ConditionDuration.Fleeting);
                RemoveByDuration(e, ConditionDuration.Temporary);
            }
        }

        private void RemoveByDuration(EntityInstance entity, ConditionDuration duration)
        {
            var toRemove = new List<string>();

            foreach (var kv in entity.conditions)
            {
                if (kv.Value.duration == duration)
                    toRemove.Add(kv.Key);
            }

            foreach (var id in toRemove)
                entity.conditions.Remove(id);
        }

        private ConditionDuration MaxDuration(ConditionDuration a, ConditionDuration b)
        {
            // Indefinite > Temporary > Fleeting
            if (a == ConditionDuration.Indefinite || b == ConditionDuration.Indefinite) return ConditionDuration.Indefinite;
            if (a == ConditionDuration.Temporary || b == ConditionDuration.Temporary) return ConditionDuration.Temporary;
            return ConditionDuration.Fleeting;
        }
    }
}
