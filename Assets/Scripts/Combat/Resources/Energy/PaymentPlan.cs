using System.Collections.Generic;

namespace Combat.Resources.Mana
{
    public class PaymentPlan
    {
        public int spendRed;
        public int spendYellow;
        public int spendBlue;

        // Optional: useful for debugging/UI later
        public List<string> choiceLog = new();

        public int TotalSpend() => spendRed + spendYellow + spendBlue;
    }
}
