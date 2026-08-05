using Combat.Entities;

namespace Combat.Statuses
{
    public class StatusSystem
    {
        public int Get(EntityInstance e, StatusId id)
        {
            if (e.statuses.TryGetValue(id, out var v)) return v;
            return 0;
        }

        public void Set(EntityInstance e, StatusId id, int value, bool clampToZero = true)
        {
            if (clampToZero && value < 0) value = 0;

            if (value == 0)
            {
                e.statuses.Remove(id);
                return;
            }

            e.statuses[id] = value;
        }

        public void Add(EntityInstance e, StatusId id, int delta, bool clampToZero = true)
        {
            int cur = Get(e, id);
            Set(e, id, cur + delta, clampToZero);
        }

        // Tick down statuses that represent "turn counters"
        public void TickDown(EntityInstance e, StatusId id, int amount = 1)
        {
            Add(e, id, -amount, clampToZero: true);
        }
    }
}
