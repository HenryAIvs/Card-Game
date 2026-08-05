using Combat.Conditions;
using Combat.Entities;
using Combat.Enemies;
using Combat.Statuses;
using Combat.Resolution;

namespace Combat.Core
{
    public enum CombatOutcome
    {
        None,
        HeroesWin,
        HeroesLose
    }

    public class CombatLoop
    {
        public CombatPhase phase = CombatPhase.StartRound;
        public CombatOutcome outcome = CombatOutcome.None;

        public bool TryEndCombat(CombatState state)
        {
            if (outcome != CombatOutcome.None) return true;

            bool heroesAllOut = AllFactionUnconscious(state, Faction.Hero);
            if (heroesAllOut)
            {
                EndCombat(state, CombatOutcome.HeroesLose);
                return true;
            }

            bool villainsAllOut = AllFactionUnconscious(state, Faction.Villain);
            if (villainsAllOut)
            {
                EndCombat(state, CombatOutcome.HeroesWin);
                return true;
            }

            return false;
        }

        private void EndCombat(CombatState state, CombatOutcome finalOutcome)
        {
            outcome = finalOutcome;
            phase = CombatPhase.EndCombat;
            state.conditions.EndCombatCleanup(state.lane.entities);
        }

        public void PrepareStartRound(CombatState state)
        {
            if (phase != CombatPhase.StartRound)
                return;

            // Passive Block from Brawn: both heroes + villains
            foreach (var e in state.lane.entities)
            {
                if (e.faction == Faction.Hero || e.faction == Faction.Villain)
                {
                    state.statuses.Add(e, StatusId.Block, e.brawn, clampToZero: true);
                }
            }

            // Hero refresh only. Drawing is now owned by CombatRunner so the
            // UI animation can block progression properly.
            foreach (var hero in state.heroes)
            {
                if (state.conditions.Has(hero, ConditionIds.Unconscious))
                    continue;

                hero.RefreshEnergyForRound();
            }
        }

        public void FinishStartRound(CombatState state)
        {
            if (phase != CombatPhase.StartRound)
                return;

            phase = CombatPhase.Heroes;
        }

        public void Advance(CombatState state)
        {
            if (phase == CombatPhase.EndCombat) return;

            switch (phase)
            {
                case CombatPhase.StartRound:
                    // StartRound is now driven by CombatRunner so it can
                    // animate card draws before entering the hero phase.
                    return;

                case CombatPhase.Heroes:
                    phase = CombatPhase.Enemies;
                    return;

                case CombatPhase.Enemies:
                {
                    EnemyTurnExecutor.ExecuteEnemyPhase(state, state.enemyDeck);
                    phase = CombatPhase.SlowResolve;
                    return;
                }

                case CombatPhase.SlowResolve:
                {
                    for (int i = 0; i < state.slowQueue.Count; i++)
                    {
                        var play = state.slowQueue[i];

                        if (state.conditions.Has(play.source, ConditionIds.Unconscious)) continue;
                        EffectExecutor.ExecuteAbility(state, play.source, play.targeting, play.effects);

                        if (state.IsCombatOver)
                            return;
                    }

                    state.slowQueue.Clear();
                    phase = CombatPhase.EndRound;
                    return;
                }

                case CombatPhase.EndRound:
                {
                    foreach (var e in state.lane.entities)
                        state.statuses.Set(e, StatusId.Block, 0);

                    foreach (var e in state.lane.entities)
                    {
                        int hidden = state.statuses.Get(e, StatusId.Hidden);
                        if (hidden > 0)
                            state.statuses.Set(e, StatusId.Hidden, hidden - 1);
                    }

                    state.conditions.EndRoundCleanup(state.lane.entities);

                    state.round += 1;
                    phase = CombatPhase.StartRound;
                    return;
                }
            }
        }

        private bool AllFactionUnconscious(CombatState state, Faction faction)
        {
            bool foundAny = false;

            foreach (var e in state.lane.entities)
            {
                if (e.faction != faction) continue;
                foundAny = true;

                if (!state.conditions.Has(e, ConditionIds.Unconscious))
                    return false;
            }

            return foundAny;
        }
    }
}