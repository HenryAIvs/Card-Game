using UnityEngine;

namespace Combat.Data.Enemies
{
    [CreateAssetMenu(menuName = "Combat/Enemy Archetype", fileName = "EnemyArchetype")]
    public class EnemyArchetypeSO : ScriptableObject
    {
        public string id;
        public string displayName;
        public Sprite laneSprite;

        public int maxHp = 5;

        public int brawn = 0;
        public int finesse = 1;
        public int ingenuity = 0;

        public EnemyAbilitySO attack;
        public EnemyAbilitySO defense;
        public EnemyAbilitySO special1;
        public EnemyAbilitySO special2;
    }
}