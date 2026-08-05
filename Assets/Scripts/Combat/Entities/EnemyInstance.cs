using Combat.Data.Enemies;
using Combat.Entities;

namespace Combat.Entities
{
    public class EnemyInstance : EntityInstance
    {
        public EnemyArchetypeSO archetype;

        public EnemyInstance(EnemyArchetypeSO archetype)
        {
            this.archetype = archetype;

            id = archetype.displayName;
            faction = Faction.Villain;
            disposition = Disposition.Hostile;
            type = EntityType.Creature;

            maxHp = archetype.maxHp;
            currentHp = archetype.maxHp;

            brawn = archetype.brawn;
            finesse = archetype.finesse;
            ingenuity = archetype.ingenuity;
        }
    }
}
