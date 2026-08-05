using Combat.Entities;
using Combat.Runner;
using UnityEngine;

namespace UI.Combat
{
    public class HeroHandBindingController
    {
        private readonly HeroHandUI owner;

        public HeroHandBindingController(HeroHandUI owner)
        {
            this.owner = owner;
        }

        public void InitialiseBindingAtStart()
        {
            UpdateBinding();
            owner.EnsureCombatRunner();

            if (owner.CombatRunner == null || !owner.CombatRunner.IsInitialised)
                owner.RefreshHandIfNeeded(force: true);
        }

        public void UpdateBinding()
        {
            RebindSelectedHero(forceRefresh: false);
        }

        public void RebindSelectedHero(bool forceRefresh)
        {
            owner.EnsureCombatRunner();

            if (owner.CombatRunner == null || !owner.CombatRunner.IsInitialised)
                return;

            owner.EnsureTargetingSession();

            HeroInstance newHero = owner.CombatRunner.GetHero(owner.SelectedHeroIndex);

            if (newHero == owner.BoundHero && !forceRefresh)
                return;

            owner.SetBoundHero(newHero);
            owner.ResetKnownHandCount();
            owner.RefreshHandIfNeeded(force: true);
        }
    }
}