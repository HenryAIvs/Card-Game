using System.Collections.Generic;
using UnityEngine;
using Combat.Data.Heroes;
using Combat.Data.Enemies;

namespace Combat.Data.Combat
{
    [CreateAssetMenu(menuName = "Combat/Combat Config", fileName = "CombatConfig")]
    public class CombatConfigSO : ScriptableObject
    {
        [Header("Heroes")]
        public List<HeroDefinitionSO> heroes = new();

        [Header("Enemies")]
        public List<EnemyArchetypeSO> enemies = new();
        public EnemyDeckSO enemyDeck;
    }
}