using Combat.Targeting.Modes;

namespace Combat.Targeting
{
    public static class TargetModeRegistry
    {
        private static readonly ITargetMode allMode = new AllTargetMode();
        private static readonly ITargetMode allOtherMode = new AllOtherTargetMode();
        private static readonly ITargetMode anyMode = new AnyTargetMode();
        private static readonly ITargetMode pickXMode = new PickXTargetMode();
        private static readonly ITargetMode adjacentMode = new AdjacentTargetMode();
        private static readonly ITargetMode anAdjacentMode = new AnAdjacentTargetMode();
        private static readonly ITargetMode nearestMode = new NearestTargetMode();
        private static readonly ITargetMode furthestMode = new FurthestTargetMode();
        private static readonly ITargetMode spaceMode = new SpaceTargetMode();

        public static ITargetMode GetMode(TargetStep step)
        {
            if (step == null)
                return anyMode;

            if (step.filter == TargetFilter.Space)
                return spaceMode;

            switch (step.specifier)
            {
                case TargetSpecifier.All:
                    return allMode;

                case TargetSpecifier.AllOther:
                    return allOtherMode;

                case TargetSpecifier.Adjacent:
                    return adjacentMode;

                case TargetSpecifier.AnAdjacent:
                    return anAdjacentMode;

                case TargetSpecifier.Nearest:
                    return nearestMode;

                case TargetSpecifier.Furthest:
                    return furthestMode;

                case TargetSpecifier.PickX:
                    return pickXMode;

                case TargetSpecifier.Any:
                default:
                    return anyMode;
            }
        }
    }
}