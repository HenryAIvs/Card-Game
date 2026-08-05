using System.Collections.Generic;
using Combat.Entities;

namespace Combat.Targeting.Modes
{
    public class NearestTargetMode : SingleEntityChoiceTargetMode
    {
        protected override List<EntityInstance> FilterCandidates(
            TargetContext context,
            List<EntityInstance> entityCandidates
        )
        {
            return TargetCandidateBuilder.GetNearestCandidates(context, entityCandidates);
        }
    }
}
