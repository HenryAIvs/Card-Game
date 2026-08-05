using System.Collections.Generic;
using UnityEngine;

namespace Combat.Data.Cards
{
    [System.Serializable]
    public class HeroDeckEntry
    {
        public HeroCardSO card;
        [Min(0)]
        public int count = 1;
    }

    [CreateAssetMenu(menuName = "Combat/Hero Deck", fileName = "HeroDeck")]
    public class HeroDeckSO : ScriptableObject
    {
        public string id;
        public string displayName;

        public List<HeroDeckEntry> entries = new();
    }
}
