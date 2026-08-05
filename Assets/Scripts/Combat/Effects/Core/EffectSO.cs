using UnityEngine;
using Combat.Core;
using Combat.Entities;
using Combat.Targeting;

namespace Combat.Data.Effects
{
    public abstract class EffectSO : ScriptableObject
    {
        public abstract string Keyword { get; }

        public abstract void Execute(
            CombatState state,
            EntityInstance source,
            TargetResult targets
        );
    }
}
