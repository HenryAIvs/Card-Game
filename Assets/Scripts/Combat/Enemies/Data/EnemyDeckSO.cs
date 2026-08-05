using System.Collections.Generic;
using UnityEngine;
using Combat.Enemies;

namespace Combat.Data.Enemies
{
    [CreateAssetMenu(menuName = "Combat/Enemy Deck", fileName = "EnemyDeck")]
    public class EnemyDeckSO : ScriptableObject
    {
        public List<EnemyCardType> cards = new();
    }
}
