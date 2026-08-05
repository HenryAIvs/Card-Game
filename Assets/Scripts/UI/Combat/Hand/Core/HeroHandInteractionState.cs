namespace UI.Combat
{
    public class HeroHandInteractionState
    {
        private readonly HeroHandUI owner;

        public HeroHandInteractionState(HeroHandUI owner)
        {
            this.owner = owner;
        }

        public bool IsAwaitingManaSelection()
        {
            HeroHandManaSelectionController manaController = owner.GetManaSelectionController();
            return manaController != null && manaController.IsAwaitingSelection;
        }

        public bool IsBusyWithCardFlow()
        {
            return IsAwaitingManaSelection()
                || (owner.TargetingSession != null && owner.TargetingSession.IsActive);
        }

        public bool IsInteractionLocked()
        {
            // Important:
            // Do not treat an active targeting session as a hard interaction lock,
            // otherwise the targeted card can never be confirmed and played.
            return (owner.CombatRunner != null && owner.CombatRunner.IsCardFlowLocked)
                || IsAwaitingManaSelection();
        }

        public bool IsDrawFlowAnimating()
        {
            return (owner.CombatRunner != null && owner.CombatRunner.IsCardFlowLocked)
                || owner.IsVisualRefreshLocked;
        }
    }
}