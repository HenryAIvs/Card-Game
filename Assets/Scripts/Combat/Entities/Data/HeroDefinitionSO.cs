using UnityEngine;
using Combat.Data.Cards;

namespace Combat.Data.Heroes
{
    [CreateAssetMenu(fileName = "HeroDefinition", menuName = "Combat/Hero Definition")]
    public class HeroDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        public string heroId = "hero";
        public string displayName = "Hero Name";
        public Sprite laneSprite;

        [Header("Base Stats")]
        public int brawn = 1;
        public int finesse = 1;
        public int ingenuity = 1;

        [Header("Starting Deck")]
        public HeroDeckSO startingDeck;
    }
}