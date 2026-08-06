using System;
using System.Collections.Generic;
using Combat.Entities;

namespace Combat.Targeting
{
    public class TargetResult
    {
        // Labels are case-insensitive so an authoring slip like "t1" vs "T1"
        // cannot silently disconnect an effect from its targets.

        // Entities selected by each step label
        public Dictionary<string, List<EntityInstance>> selections = new(StringComparer.OrdinalIgnoreCase);

        // Spaces selected by each step label (insertion indices)
        public Dictionary<string, List<int>> spaces = new(StringComparer.OrdinalIgnoreCase);

        public List<EntityInstance> GetEntities(string label)
        {
            return selections.TryGetValue(label, out var list) ? list : new List<EntityInstance>();
        }

        public List<int> GetSpaces(string label)
        {
            return spaces.TryGetValue(label, out var list) ? list : new List<int>();
        }

        public void SetEntities(string label, List<EntityInstance> entities)
        {
            selections[label] = entities;
        }

        public void SetSpaces(string label, List<int> indices)
        {
            spaces[label] = indices;
        }
    }
}
