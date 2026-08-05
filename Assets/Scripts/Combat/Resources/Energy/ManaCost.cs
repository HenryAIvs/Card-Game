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

        public ManaCost() { }

        // Convenience helpers 
        public ManaCost AddForced(ManaColor color, int amount)
        {
            costs.Add(new ChoiceCost(new[] { color }, amount));
            return this;
        }

        public ManaCost AddFlex(ManaColor a, ManaColor b, int amount)
        {
            costs.Add(new ChoiceCost(new[] { a, b }, amount));
            return this;
        }

        public ManaCost AddGeneric(int amount)
        {
            costs.Add(new ChoiceCost(new[] { ManaColor.Red, ManaColor.Yellow, ManaColor.Blue }, amount));
            return this;
        }
    }
}
