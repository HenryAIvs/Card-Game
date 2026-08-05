using System.Collections.Generic;
using Combat.Conditions;
using Combat.Statuses;

namespace Combat.Entities
{
    public enum Faction
    {
        Hero,
        Villain,
        Neutral
    }

    public enum Disposition
    {
        Friendly,
        Hostile,
        Passive
    }

    public enum EntityType
    {
        Creature,
        Environment
    }

    public class EntityInstance
    {
        public string id;

        public Faction faction;
        public Disposition disposition;
        public EntityType type;

        // Enemy HP (heroes may use later if you want)
        public int maxHp;
        public int currentHp;

        // Stats
        public int brawn;
        public int finesse;
        public int ingenuity;

        // Debuff conditions (Unconscious, Exhaustion, etc.)
        public Dictionary<string, ConditionInstance> conditions = new();

        // Statuses (Block, Armour, Ward, Hidden) are ALL here now
        public Dictionary<StatusId, int> statuses = new();
    }
}
