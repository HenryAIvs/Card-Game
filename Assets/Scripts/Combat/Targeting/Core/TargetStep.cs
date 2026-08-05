namespace Combat.Targeting
{
    [System.Serializable]
    public class TargetStep
    {
        public TargetSpecifier specifier;
        public TargetFilter filter;

        // For PickX
        public int pickCount = 1;

        // Store results under this label (e.g. "T1", "T2", "S1")
        public string label = "T";

        // Anchor for distance/adjacency calculations
        public TargetAnchor anchor = TargetAnchor.Source;

        // If anchor == Label, use entities from this label as anchors (e.g. "T1")
        public string anchorLabel = "T1";

        // If filter == Target, pull candidates from this label
        public string targetLabel = "T1";
    }
}