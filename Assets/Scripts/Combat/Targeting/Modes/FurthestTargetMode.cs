using System.Collections.Generic;
using Combat.Entities;

namespace Combat.Targeting.Modes
{
    public class FurthestTargetMode : ITargetMode
    {
        public bool RequiresManualChoice(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            List<EntityInstance> furthest = TargetCandidateBuilder.GetFurthestCandidates(
                context,
                entityCandidates
            );

            return furthest.Count > 1;
        }

        public void AutoResolve(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            List<EntityInstance> furthest = TargetCandidateBuilder.GetFurthestCandidates(
                context,
                entityCandidates
            );

            List<EntityInstance> selected = new List<EntityInstance>();
            if (furthest.Count > 0)
                selected.Add(furthest[0]);

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
            List<EntityInstance> furthest = TargetCandidateBuilder.GetFurthestCandidates(
                context,
                entityCandidates
            );

            if (chooseEntities == null)
            {
                List<EntityInstance> selected = new List<EntityInstance>();
                if (furthest.Count > 0)
                    selected.Add(furthest[0]);

                context.result.SetEntities(context.step.label, selected);
                return;
            }

            List<EntityInstance> selectedFromChoice = chooseEntities(furthest, 1) ?? new List<EntityInstance>();
            context.result.SetEntities(context.step.label, selectedFromChoice);
        }
    }
}