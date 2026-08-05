using System.Collections.Generic;
using Combat.Entities;

namespace Combat.Targeting.Modes
{
    public class AnAdjacentTargetMode : SingleEntityChoiceTargetMode
    {
        protected override List<EntityInstance> FilterCandidates(
            TargetContext context,
            List<EntityInstance> entityCandidates
        )
        {
            return TargetCandidateBuilder.GetAdjacentCandidates(context, entityCandidates);
        }
    }
}
