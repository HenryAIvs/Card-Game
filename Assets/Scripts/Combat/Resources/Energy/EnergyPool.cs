namespace Combat.Resources.Mana
{
    public struct EnergyPool
    {
        public int red;
        public int yellow;
        public int blue;

        public EnergyPool(int red, int yellow, int blue)
        {
            this.red = red;
            this.yellow = yellow;
            this.blue = blue;
        }

        public int Get(ManaColor c)
        {
            return c switch
            {
                ManaColor.Red => red,
                ManaColor.Yellow => yellow,
                ManaColor.Blue => blue,
                _ => 0
            };
        }

        public void Spend(ManaColor c, int amount)
        {
            if (amount <= 0) return;

            switch (c)
            {
                case ManaColor.Red: red -= amount; break;
                case ManaColor.Yellow: yellow -= amount; break;
                case ManaColor.Blue: blue -= amount; break;
            }
        }
    }
}
