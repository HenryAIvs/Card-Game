using System.Collections.Generic;
using Combat.Entities;

namespace Combat.Targeting.Modes
{
    public class AnyTargetMode : ITargetMode
    {
        public bool RequiresManualChoice(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            return entityCandidates.Count > 1;
        }

        public void AutoResolve(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            List<EntityInstance> selected = new List<EntityInstance>();

            if (entityCandidates.Count > 0)
                selected.Add(entityCandidates[0]);

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
            if (chooseEntities == null)
            {
                AutoResolve(context, entityCandidates, spaceCandidates);
                return;
            }

            List<EntityInstance> selected = chooseEntities(entityCandidates, 1) ?? new List<EntityInstance>();
            context.result.SetEntities(context.step.label, selected);
        }
    }
}