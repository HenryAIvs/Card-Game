using System.Collections.Generic;
using Combat.Entities;

namespace Combat.Targeting.Modes
{
    public class AllTargetMode : ITargetMode
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
            context.result.SetEntities(context.step.label, new List<EntityInstance>(entityCandidates));
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