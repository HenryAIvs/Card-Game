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

            // Rosters, not the lane: the lane drops the dead, so outcome
            // checks must count everyone who entered combat.
            bool heroesAllOut = AllUnconscious(state, state.heroes);
            if (heroesAllOut)
            {
                EndCombat(state, CombatOutcome.HeroesLose);
                return true;
            }

            bool villainsAllOut = AllUnconscious(state, state.enemies);
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

            // Passive Block from Brawn. Heroes get theirs at round start;
            // villains get theirs when their own turn begins.
            foreach (var e in state.lane.entities)
            {
                if (e.faction == Faction.Hero)
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

        public void FinishEnemyPhase(CombatState state)
        {
            if (phase != CombatPhase.Enemies)
                return;

            phase = CombatPhase.SlowResolve;
        }

        public void FinishSlowResolve(CombatState state)
        {
            if (phase != CombatPhase.SlowResolve)
                return;

            state.slowQueue.Clear();
            phase = CombatPhase.EndRound;
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
                    // The enemy phase is driven by CombatRunner so each draw
                    // and card play can be shown before its effects resolve.
                    return;

                case CombatPhase.SlowResolve:
                    // Driven by CombatRunner so each slow card can be shown
                    // before its effects resolve.
                    return;

                case CombatPhase.EndRound:
                {
                    // Villain block persists into the next round and resets at
                    // the start of their own turn instead.
                    foreach (var e in state.lane.entities)
                    {
                        if (e.faction != Faction.Villain)
                            state.statuses.Set(e, StatusId.Block, 0);
                    }

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

        private bool AllUnconscious<T>(CombatState state, System.Collections.Generic.List<T> roster)
            where T : EntityInstance
        {
            bool foundAny = false;

            foreach (var e in roster)
            {
                if (e == null) continue;
                foundAny = true;

                if (!state.conditions.Has(e, ConditionIds.Unconscious))
                    return false;
            }

            return foundAny;
        }
    }
}