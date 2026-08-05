namespace Combat.Conditions
{
    public class ConditionInstance
    {
        public string id;
        public ConditionDuration duration;

        public ConditionInstance(string id, ConditionDuration duration)
        {
            this.id = id;
            this.duration = duration;
        }
    }
}
