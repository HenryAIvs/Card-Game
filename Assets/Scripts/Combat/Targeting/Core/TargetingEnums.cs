namespace Combat.Targeting
{
    public enum TargetFilter
    {
        Self,
        Ally,
        Adversary,
        Neutral,
        Any,
        // Special: use entities stored under another label (e.g. "T1")
        Target,
        // Special: insertion indices (0..lane.Count)
        Space
    }

    public enum TargetSpecifier
    {
        Any,
        All,
        AllOther,
        Adjacent,
        AnAdjacent,
        Nearest,
        Furthest,
        PickX
        // Triggering later
    }

    //where distance/adjacency is measured from
    public enum TargetAnchor
    {
        Source, // relative to the entity playing the effect
        Label   // relative to entities selected in a previous step label
    }
}
