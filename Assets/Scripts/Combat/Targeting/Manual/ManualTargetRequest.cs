using System.Collections.Generic;
using Combat.Entities;

namespace Combat.Targeting
{
    public class ManualTargetRequest
    {
        public int manualStepIndex;
        public TargetStep step;

        public List<EntityInstance> validTargets = new List<EntityInstance>();
        public List<int> validSpaces = new List<int>();

        public bool IsSpaceRequest => step != null && step.filter == TargetFilter.Space;
        public bool IsEntityRequest => !IsSpaceRequest;
    }
}