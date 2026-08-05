using System.Collections.Generic;
using Combat.Entities;

namespace Combat.Targeting.Modes
{
    public class NearestTargetMode : ITargetMode
    {
        public bool RequiresManualChoice(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            List<EntityInstance> nearest = TargetCandidateBuilder.GetNearestCandidates(
                context,
                entityCandidates
            );

            return nearest.Count > 1;
        }

        public void AutoResolve(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            List<EntityInstance> nearest = TargetCandidateBuilder.GetNearestCandidates(
                context,
                entityCandidates
            );

            List<EntityInstance> selected = new List<EntityInstance>();
            if (nearest.Count > 0)
                selected.Add(nearest[0]);

            context.result.SetEntities(context.step.label, selected);
        }

        public void Resolve(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates,
            TargetResolver.ChooseEntitiesFn chooseEntities,
            TargetResolver.ChooseSpacesFn chooseSpaces
        )
        {
            List<EntityInstance> nearest = TargetCandidateBuilder.GetNearestCandidates(
                context,
                entityCandidates
            );

            if (chooseEntities == null)
            {
                List<EntityInstance> selected = new List<EntityInstance>();
                if (nearest.Count > 0)
                    selected.Add(nearest[0]);

                context.result.SetEntities(context.step.label, selected);
                return;
            }

            List<EntityInstance> selectedFromChoice = chooseEntities(nearest, 1) ?? new List<EntityInstance>();
            context.result.SetEntities(context.step.label, selectedFromChoice);
        }
    }
}