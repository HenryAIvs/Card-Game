using System.Collections.Generic;
using Combat.Conditions;
using Combat.Entities;
using Combat.Enemies;
using Combat.Lane;
using Combat.Statuses;
using Combat.Cards;

namespace Combat.Core
{
    public class CombatState
    {
        public int round;
        public CombatLane lane = new();
        public CombatLoop loop = new();

        public ConditionSystem conditions = new();
        public StatusSystem statuses = new();
        public List<SlowCardPlay> slowQueue = new();
        public List<HeroInstance> heroes = new();

        // Full enemy roster. Unlike the lane (which drops the dead), this
        // persists so win/loss checks can count the fallen.
        public List<EnemyInstance> enemies = new();
        public EnemyDeckState enemyDeck = new EnemyDeckState();

        public bool IsCombatOver => loop.phase == CombatPhase.EndCombat;

        public CombatState()
        {
            round = 1;
            loop.phase = CombatPhase.StartRound;
        }
    }
}
