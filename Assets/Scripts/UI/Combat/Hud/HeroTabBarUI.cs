using System;
using System.Collections.Generic;
using UnityEngine;
using Combat.Runner;
using Combat.Entities;

namespace UI.Combat
{
    public class HeroTabBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform tabContainer;
        [SerializeField] private HeroTabUI tabPrefab;

        private readonly List<HeroTabUI> spawnedTabs = new List<HeroTabUI>();
        private Action<int> onTabClicked;
        private int selectedIndex = -1;

        public void BuildTabs(CombatRunner combatRunner, Action<int> onHeroTabClicked)
        {
            onTabClicked = onHeroTabClicked;
            ClearTabs();

            if (combatRunner == null || !combatRunner.IsInitialised)
                return;

            if (tabContainer == null || tabPrefab == null)
                return;

            int heroCount = combatRunner.GetHeroCount();

            for (int i = 0; i < heroCount; i++)
            {
                HeroInstance hero = combatRunner.GetHero(i);
                if (hero == null)
                    continue;

                int heroIndex = i;
                HeroTabUI tab = Instantiate(tabPrefab, tabContainer);
                tab.Bind(heroIndex, hero.id, HandleTabClicked);
                spawnedTabs.Add(tab);
            }

            SetSelectedIndex(0);
        }

        public void RefreshNamesIfNeeded(CombatRunner combatRunner)
        {
            if (combatRunner == null || !combatRunner.IsInitialised)
                return;

            for (int i = 0; i < spawnedTabs.Count; i++)
            {
                HeroTabUI tab = spawnedTabs[i];
                if (tab == null)
                    continue;

                HeroInstance hero = combatRunner.GetHero(i);
                if (hero == null)
                    continue;

                tab.SetName(hero.id);
            }
        }

        public void SetSelectedIndex(int newSelectedIndex)
        {
            selectedIndex = newSelectedIndex;

            for (int i = 0; i < spawnedTabs.Count; i++)
            {
                HeroTabUI tab = spawnedTabs[i];
                if (tab == null)
                    continue;

                tab.SetSelected(i == selectedIndex);
            }
        }

        private void HandleTabClicked(int heroIndex)
        {
            onTabClicked?.Invoke(heroIndex);
        }

        private void ClearTabs()
        {
            for (int i = spawnedTabs.Count - 1; i >= 0; i--)
            {
                if (spawnedTabs[i] != null)
                    Destroy(spawnedTabs[i].gameObject);
            }

            spawnedTabs.Clear();
        }
    }
}