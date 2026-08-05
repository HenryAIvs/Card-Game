using System.Collections.Generic;
using UnityEngine;
using Combat.Data.Mana;
using Combat.Data.Effects;
using Combat.Targeting;
using Combat.Cards;

namespace Combat.Data.Cards
{
    [CreateAssetMenu(menuName = "Combat/Hero Card", fileName = "HeroCard")]
    public class HeroCardSO : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;

        [TextArea(3, 6)]
        public string rulesText;

        [Header("Cost")]
        public ManaCostData cost = new ManaCostData();

        [Header("Gameplay")]
        // Ordered targeting steps
        public List<TargetStep> targeting = new();

        // Ordered effects. Executed in order.
        public List<EffectSO> effects = new();

        public List<CardTag> tags = new();
    }
}