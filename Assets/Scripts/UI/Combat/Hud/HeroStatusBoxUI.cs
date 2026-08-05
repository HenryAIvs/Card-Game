using System;
using UnityEngine;
using TMPro;
using Combat.Entities;
using Combat.Resources.Mana;

namespace UI.Combat
{
    public class HeroStatusBoxUI : MonoBehaviour
    {
        public static event Action<HeroInstance, ManaColor> OnEnergyClicked;

        [Header("Text")]
        [SerializeField] private TMP_Text heroNameText;

        [Header("Energy")]
        [SerializeField] private Transform energyBar;
        [SerializeField] private EnergyCircleUI energyCirclePrefab;

        private HeroInstance hero;

        private string lastHeroName = null;

        private int lastMaxRed = -1;
        private int lastMaxYellow = -1;
        private int lastMaxBlue = -1;

        private int lastCurrentRed = -1;
        private int lastCurrentYellow = -1;
        private int lastCurrentBlue = -1;

        public void Bind(HeroInstance heroInstance)
        {
            hero = heroInstance;
            ForceFullRefresh();
        }

        public void Refresh()
        {
            if (hero == null)
                return;

            RefreshNameIfNeeded();
            RefreshEnergyIfNeeded();
        }

        private void ForceFullRefresh()
        {
            if (hero == null)
                return;

            lastHeroName = null;

            lastMaxRed = -1;
            lastMaxYellow = -1;
            lastMaxBlue = -1;

            lastCurrentRed = -1;
            lastCurrentYellow = -1;
            lastCurrentBlue = -1;

            Refresh();
        }

        private void RefreshNameIfNeeded()
        {
            string currentName = string.IsNullOrWhiteSpace(hero.id) ? "Hero Name" : hero.id;

            if (currentName == lastHeroName)
                return;

            if (heroNameText != null)
                heroNameText.text = currentName;

            lastHeroName = currentName;
        }

        private void RefreshEnergyIfNeeded()
        {
            if (energyBar == null || energyCirclePrefab == null)
                return;

            if (!HasEnergyChanged())
                return;

            RebuildEnergyBar();
            CacheEnergyValues();
        }

        private bool HasEnergyChanged()
        {
            return
                hero.energyMax.red != lastMaxRed ||
                hero.energyMax.yellow != lastMaxYellow ||
                hero.energyMax.blue != lastMaxBlue ||
                hero.energyCurrent.red != lastCurrentRed ||
                hero.energyCurrent.yellow != lastCurrentYellow ||
                hero.energyCurrent.blue != lastCurrentBlue;
        }

        private void CacheEnergyValues()
        {
            lastMaxRed = hero.energyMax.red;
            lastMaxYellow = hero.energyMax.yellow;
            lastMaxBlue = hero.energyMax.blue;

            lastCurrentRed = hero.energyCurrent.red;
            lastCurrentYellow = hero.energyCurrent.yellow;
            lastCurrentBlue = hero.energyCurrent.blue;
        }

        private void RebuildEnergyBar()
        {
            ClearEnergyBar();

            SpawnEnergyGroup(ManaColor.Red, hero.energyMax.red, hero.energyCurrent.red);
            SpawnEnergyGroup(ManaColor.Yellow, hero.energyMax.yellow, hero.energyCurrent.yellow);
            SpawnEnergyGroup(ManaColor.Blue, hero.energyMax.blue, hero.energyCurrent.blue);
        }

        private void SpawnEnergyGroup(ManaColor color, int maxAmount, int currentAmount)
        {
            for (int i = 0; i < maxAmount; i++)
            {
                bool isFilled = i < currentAmount;
                SpawnEnergyCircle(color, isFilled);
            }
        }

        private void SpawnEnergyCircle(ManaColor color, bool isFilled)
        {
            EnergyCircleUI circle = Instantiate(energyCirclePrefab, energyBar);
            circle.SetVisual(color, isFilled);
            circle.SetClickable(isFilled, HandleEnergyClicked);
        }

        private void HandleEnergyClicked(ManaColor color, bool isFilled)
        {
            if (!isFilled || hero == null)
                return;

            OnEnergyClicked?.Invoke(hero, color);
        }

        private void ClearEnergyBar()
        {
            for (int i = energyBar.childCount - 1; i >= 0; i--)
            {
                Destroy(energyBar.GetChild(i).gameObject);
            }
        }
    }
}