using System;
using System.Collections.Generic;
using Combat.Entities;

namespace Combat.Lane
{
    public class CombatLane
    {
        public List<EntityInstance> entities = new();

        public int IndexOf(EntityInstance entity) => entities.IndexOf(entity);

        public void AddAt(int index, EntityInstance entity)
        {
            index = Math.Clamp(index, 0, entities.Count);
            entities.Insert(index, entity);
        }

        public void Remove(EntityInstance entity)
        {
            entities.Remove(entity);
        }

        // Swap-through movement rule:
        // Remove + Insert naturally shifts everything you passed by 1.
        public void MoveSwapThrough(EntityInstance entity, int delta)
        {
            int from = IndexOf(entity);
            if (from < 0) return;
            if (delta == 0) return;

            int desired = from + delta;
            int to = Math.Clamp(desired, 0, entities.Count - 1);

            if (to == from) return;

            entities.RemoveAt(from);

            int insertIndex = Math.Clamp(to, 0, entities.Count);
            entities.Insert(insertIndex, entity);
        }

        public void MoveToIndexSwapThrough(EntityInstance entity, int toIndex)
        {
            int from = IndexOf(entity);
            if (from < 0) return;

            int to = Math.Clamp(toIndex, 0, entities.Count - 1);
            if (to == from) return;

            entities.RemoveAt(from);
            int insertIndex = Math.Clamp(to, 0, entities.Count);
            entities.Insert(insertIndex, entity);
        }

        public void MoveToGapSwapThrough(EntityInstance entity, int gapIndex)
        {
            int from = IndexOf(entity);
            if (from < 0) return;

            int clampedGap = Math.Clamp(gapIndex, 0, entities.Count);

            // The entity occupies the span between gaps:
            // from and from + 1
            // Choosing either is a no-op.
            if (clampedGap == from || clampedGap == from + 1)
                return;

            entities.RemoveAt(from);

            // After removal, gaps to the right shift left by 1.
            int insertIndex = clampedGap <= from
                ? clampedGap
                : clampedGap - 1;

            insertIndex = Math.Clamp(insertIndex, 0, entities.Count);
            entities.Insert(insertIndex, entity);
        }

        public IEnumerable<EntityInstance> GetAll() => entities;
    }
}