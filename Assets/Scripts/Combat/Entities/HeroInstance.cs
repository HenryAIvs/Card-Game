using Combat.Cards;
using Combat.Resources.Mana;
using Combat.Data.Heroes;

namespace Combat.Entities
{
    public class HeroInstance : EntityInstance
    {
        public HeroDefinitionSO definition;

        // Per-hero deck state: draw pile, hand, discard pile
        public DeckState deck = new DeckState();

        // Energy for this round
        public EnergyPool energyMax;
        public EnergyPool energyCurrent;

        public HeroInstance()
        {
            faction = Faction.Hero;
            disposition = Disposition.Friendly;
            type = EntityType.Creature;
        }

        public void RecalculateEnergyMax()
        {
            // 1 stat = 1 energy of that color
            energyMax = new EnergyPool(
                red: brawn,
                yellow: finesse,
                blue: ingenuity
            );
        }

        public void RefreshEnergyForRound()
        {
            RecalculateEnergyMax();
            energyCurrent = energyMax;
        }

        public int GetRoundDrawCount(Combat.Core.CombatState state)
        {
            int baseDraw = state != null && state.round == 1 ? 4 : 2;
            int draw = baseDraw + finesse;

            // Exhaustion = draw 1 fewer card at start of turn
            if (state.conditions.Has(this, Combat.Conditions.ConditionIds.Exhaustion))
                draw -= 1;

            if (draw < 0)
                draw = 0;

            return draw;
        }
    }
}