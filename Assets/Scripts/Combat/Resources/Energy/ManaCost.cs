using System.Collections.Generic;

namespace Combat.Resources.Mana
{
    public struct ChoiceCost
    {
        public ManaColor[] options;
        public int amount;

        public ChoiceCost(ManaColor[] options, int amount)
        {
            this.options = options;
            this.amount = amount;
        }
    }

    public class ManaCost
    {
        public List<ChoiceCost> costs = new();
    }
}
