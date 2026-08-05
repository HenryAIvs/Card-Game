using UnityEngine;
using Combat.Core;
using Combat.Entities;
using Combat.Targeting;

namespace Combat.Data.Effects
{
    [CreateAssetMenu(menuName = "Combat/Effects/Move Up To", fileName = "Effect_MoveUpTo")]
    public class MoveUpToEffectSO : EffectSO
    {
        public override string Keyword => "MoveUpTo";

        // Maximum gaps crossed in either direction.
        public int maxDistance = 1;

        // Entity/entities to move.
        public string targetLabel = "T1";

        // Chosen destination gap label.
        public string spaceLabel = "S1";

        // Fallback when no chosen gap exists.
        public int defaultDelta = 0;

        public override void Execute(CombatState state, EntityInstance source, TargetResult targets)
        {
            if (state == null || state.lane == null || targets == null)
                return;

            var entities = targets.GetEntities(targetLabel);
            var chosenSpaces = targets.GetSpaces(spaceLabel);

            for (int i = 0; i < entities.Count; i++)
            {
                EntityInstance entity = entities[i];
                if (entity == null)
                    continue;

                if (chosenSpaces.Count > 0)
                {
                    int chosenGap = chosenSpaces[Mathf.Min(i, chosenSpaces.Count - 1)];
                    int currentIndex = state.lane.IndexOf(entity);

                    if (currentIndex < 0)
                        continue;

                    int distance = TargetCandidateBuilder.GetGapDistanceFromEntity(currentIndex, chosenGap);
                    if (distance <= 0)
                        continue;

                    if (distance > maxDistance)
                        continue;

                    state.lane.MoveToGapSwapThrough(entity, chosenGap);
                    continue;
                }

                int delta = Mathf.Clamp(defaultDelta, -maxDistance, maxDistance);
                state.lane.MoveSwapThrough(entity, delta);
            }
        }
    }
}