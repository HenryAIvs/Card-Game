                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        using UnityEngine;
using Combat.Runner;
using Combat.Entities;

namespace UI.Combat
{
    public class CombatHudUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatRunner combatRunner;
        [SerializeField] private HeroHandUI heroHandUI;
        [SerializeField] private HeroTabBarUI heroTabBar;
        [SerializeField] private SelectedHeroEnergyUI selectedHeroEnergyUI;

        private bool hasInitialisedUi = false;
        private HeroInstance lastEnergyHero = null;

        private void Start()
        {
            TryInitialiseUi();
            RefreshAll();
        }

        private void Update()
        {
            if (!hasInitialisedUi)
            {
                TryInitialiseUi();
            }

            RefreshAll();
        }

        private void TryInitialiseUi()
        {
            if (hasInitialisedUi)
                return;

            if (combatRunner == null)
                combatRunner = FindFirstObjectByType<CombatRunner>();

            if (heroHandUI == null)
                heroHandUI = FindFirstObjectByType<HeroHandUI>();

            if (combatRunner == null || heroHandUI == null)
                return;

            if (!combatRunner.IsInitialised)
                return;

            if (heroTabBar != null)
            {
                heroTabBar.BuildTabs(combatRunner, HandleHeroTabClicked);
            }

            heroHandUI.SetSelectedHeroIndex(0);

            // Hosts for the code-built overlays; nothing in the scene
            // references them, so they are created here on first init.
            if (FindFirstObjectByType<ScryOverlayUI>() == null)
                new GameObject("ScryOverlayUI").AddComponent<ScryOverlayUI>();

            if (FindFirstObjectByType<TargetChoicePanelUI>() == null)
                new GameObject("TargetChoicePanelUI").AddComponent<TargetChoicePanelUI>();

            if (FindFirstObjectByType<CombatEndUI>() == null)
                new GameObject("CombatEndUI").AddComponent<CombatEndUI>();

            hasInitialisedUi = true;
        }

        private void HandleHeroTabClicked(int heroIndex)
        {
            if (heroHandUI == null)
                return;

            if (heroHandUI.IsBusyWithCardFlow)
                return;

            heroHandUI.SetSelectedHeroIndex(heroIndex);

            if (heroTabBar != null)
                heroTabBar.SetSelectedIndex(heroIndex);
        }

        public void RefreshAll()
        {
            if (!hasInitialisedUi)
                return;

            if (heroTabBar != null && heroHandUI != null)
            {
                heroTabBar.SetSelectedIndex(heroHandUI.SelectedHeroIndex);
                heroTabBar.RefreshNamesIfNeeded(combatRunner);
            }

            if (selectedHeroEnergyUI != null && heroHandUI != null)
            {
                HeroInstance currentHero = heroHandUI.BoundHero;

                if (currentHero != lastEnergyHero)
                {
                    selectedHeroEnergyUI.Bind(currentHero);
                    lastEnergyHero = currentHero;
                }
                else
                {
                    selectedHeroEnergyUI.Refresh();
                }

                // Flash the circles the player may pick for a pending mana cost.
                HeroHandManaSelectionController manaSelection = heroHandUI.GetManaSelectionController();
                selectedHeroEnergyUI.SetPendingPickOptions(
                    manaSelection != null ? manaSelection.GetCurrentChoiceOptions() : null
                );
            }
        }
    }
}