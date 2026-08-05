using System.Collections.Generic;
using Combat.Entities;

namespace Combat.Targeting.Modes
{
    public class AnAdjacentTargetMode : ITargetMode
    {
        public bool RequiresManualChoice(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            List<EntityInstance> adjacent = TargetCandidateBuilder.GetAdjacentCandidates(
                context,
                entityCandidates
            );

            return adjacent.Count > 1;
        }

        public void AutoResolve(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            List<EntityInstance> adjacent = TargetCandidateBuilder.GetAdjacentCandidates(
                context,
                entityCandidates
            );

            List<EntityInstance> selected = new List<EntityInstance>();
            if (adjacent.Count > 0)
                selected.Add(adjacent[0]);

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
            List<EntityInstance> adjacent = TargetCandidateBuilder.GetAdjacentCandidates(
                context,
                entityCandidates
            );

            if (chooseEntities == null)
            {
                List<EntityInstance> selected = new List<EntityInstance>();
                if (adjacent.Count > 0)
                    selected.Add(adjacent[0]);

                context.result.SetEntities(context.step.label, selected);
                return;
            }

            List<EntityInstance> selectedFromChoice = chooseEntities(adjacent, 1) ?? new List<EntityInstance>();
            context.result.SetEntities(context.step.label, selectedFromChoice);
        }
    }
}