using System.Collections.Generic;
using System.Linq;
using Combat.Entities;
using UnityEngine;

namespace Combat.Targeting.Modes
{
    public class PickXTargetMode : ITargetMode
    {
        public bool RequiresManualChoice(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            int needed = Mathf.Max(1, context.step.pickCount);
            return entityCandidates.Count > needed;
        }

        public void AutoResolve(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            int needed = Mathf.Max(1, context.step.pickCount);
            List<EntityInstance> selected = entityCandidates.Take(needed).ToList();
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
            int needed = Mathf.Max(1, context.step.pickCount);

            if (chooseEntities == null)
            {
                AutoResolve(context, entityCandidates, spaceCandidates);
                return;
            }

            List<EntityInstance> selected = chooseEntities(entityCandidates, needed) ?? new List<EntityInstance>();
            context.result.SetEntities(context.step.label, selected);
        }
    }
}