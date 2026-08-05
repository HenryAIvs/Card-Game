using System.Collections.Generic;
using System.Linq;
using Combat.Entities;

namespace Combat.Targeting.Modes
{
    public class AllOtherTargetMode : ITargetMode
    {
        public bool RequiresManualChoice(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            return false;
        }

        public void AutoResolve(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            List<EntityInstance> selected = entityCandidates
                .Where(e => e != context.source)
                .ToList();

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
            AutoResolve(context, entityCandidates, spaceCandidates);
        }
    }
}