using System.Collections.Generic;
using UnityEngine;
using Combat.Resources.Mana;

namespace Combat.Data.Mana
{
    [System.Serializable]
    public class ChoiceCostData
    {
        public List<ManaColor> options = new();
        public int amount = 1;
    }

    [System.Serializable]
    public class ManaCostData
    {
        public List<ChoiceCostData> costs = new();

        public ManaCost ToRuntime()
        {
            var rt = new ManaCost();
            foreach (var c in costs)
            {
                rt.costs.Add(new ChoiceCost(c.options.ToArray(), c.amount));
            }
            return rt;
        }
    }
}
