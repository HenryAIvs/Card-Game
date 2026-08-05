using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Combat.Entities;

namespace Combat.Targeting.Modes
{
    public class SpaceTargetMode : ITargetMode
    {
        public bool RequiresManualChoice(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            switch (context.step.specifier)
            {
                case TargetSpecifier.All:
                    return false;

                case TargetSpecifier.PickX:
                {
                    int needed = Mathf.Max(1, context.step.pickCount);
                    return spaceCandidates.Count > needed;
                }

                case TargetSpecifier.Any:
                default:
                    return spaceCandidates.Count > 1;
            }
        }

        public void AutoResolve(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates
        )
        {
            List<int> selected = new List<int>();

            switch (context.step.specifier)
            {
                case TargetSpecifier.All:
                    selected = new List<int>(spaceCandidates);
                    break;

                case TargetSpecifier.PickX:
                {
                    int needed = Mathf.Max(1, context.step.pickCount);
                    selected = spaceCandidates.Take(needed).ToList();
                    break;
                }

                case TargetSpecifier.Any:
                default:
                    if (spaceCandidates.Count > 0)
                        selected.Add(spaceCandidates[0]);
                    break;
            }

            context.result.SetSpaces(context.step.label, selected);
        }

        public void Resolve(
            TargetContext context,
            List<EntityInstance> entityCandidates,
            List<int> spaceCandidates,
            TargetResolver.ChooseEntitiesFn chooseEntities,
            TargetResolver.ChooseSpacesFn chooseSpaces
        )
        {
            if (chooseSpaces == null)
            {
                AutoResolve(context, entityCandidates, spaceCandidates);
                return;
            }

            List<int> selected = new List<int>();

            switch (context.step.specifier)
            {
                case TargetSpecifier.All:
                    selected = new List<int>(spaceCandidates);
                    break;

                case TargetSpecifier.PickX:
                {
                    int needed = Mathf.Max(1, context.step.pickCount);
                    selected = chooseSpaces(spaceCandidates, needed) ?? new List<int>();
                    break;
                }

                case TargetSpecifier.Any:
                default:
                    selected = chooseSpaces(spaceCandidates, 1) ?? new List<int>();
                    break;
            }

            context.result.SetSpaces(context.step.label, selected);
        }
    }
}