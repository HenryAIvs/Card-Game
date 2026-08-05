using System.Collections.Generic;
using UnityEngine;
using Combat.Data.Effects;
using Combat.Targeting;

namespace Combat.Data.Enemies
{
    [CreateAssetMenu(menuName = "Combat/Enemy Ability", fileName = "EnemyAbility")]
    public class EnemyAbilitySO : ScriptableObject
    {
        public string id;
        public string displayName;

        // Ordered targeting steps
        public List<TargetStep> targeting = new();

        // NEW: keyword/effect list (Hit, Move, etc.)
        public List<EffectSO> effects = new();
    }
}
