using System.Collections.Generic;
using Combat.Entities;

namespace Combat.Targeting
{
    public interface ITargetMode
    {
        bool RequiresManualChoice(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        );

        void AutoResolve(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        );

        void Resolve(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates,
            TargetResolver.ChooseEntitiesFn chooseEntities,
            TargetResolver.ChooseSpacesFn chooseSpaces
        );
    }

    // Shared behaviour for modes that pick a single entity from a filtered pool
    // (Any, AnAdjacent, Nearest, Furthest). Subclasses only supply the filter.
    public abstract class SingleEntityChoiceTargetMode : ITargetMode
    {
        protected abstract List<EntityInstance> FilterCandidates(
            TargetContext context,
            List<EntityInstance> entityCandidates
        );

        public bool RequiresManualChoice(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            return FilterCandidates(context, entityCandidates).Count > 1;
        }

        public void AutoResolve(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            List<EntityInstance> filtered = FilterCandidates(context, entityCandidates);

            List<EntityInstance> selected = new List<EntityInstance>();
            if (filtered.Count > 0)
                selected.Add(filtered[0]);

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

            List<EntityInstance> filtered = FilterCandidates(context, entityCandidates);
            List<EntityInstance> selected = chooseEntities(filtered, 1) ?? new List<EntityInstance>();
            context.result.SetEntities(context.step.label, selected);
        }
    }
}
