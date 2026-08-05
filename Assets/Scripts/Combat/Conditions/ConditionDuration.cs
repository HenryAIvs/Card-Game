namespace Combat.Conditions
{
    public enum ConditionDuration
    {
        Fleeting,   // expires at end of round
        Temporary,  // expires at end of combat
        Indefinite  // expires only when removed
    }
}
