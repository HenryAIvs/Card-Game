using System.Collections.Generic;
using Combat.Entities;

namespace Combat.Targeting
{
    public class ManualTargetRequest
    {
        public int manualStepIndex;
        public TargetStep step;

        // Steps before manualStepIndex that auto-resolved while building this
        // request. The targeting session must start from these, or later
        // steps anchored on an auto-resolved label cannot validate.
        public TargetResult resolvedSoFar;

        public List<EntityInstance> validTargets = new List<EntityInstance>();
        public List<int> validSpaces = new List<int>();

        public bool IsSpaceRequest => step != null && step.filter == TargetFilter.Space;
        public bool IsEntityRequest => !IsSpaceRequest;
    }
}