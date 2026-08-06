using System;
using UnityEngine;
using UnityEngine.UI;
using Combat.Entities;
using Combat.Resources.Mana;

namespace UI.Combat
{
    public class SelectedHeroEnergyUI : MonoBehaviour
    {
        public static event Action<HeroInstance, ManaColor> OnEnergyClicked;

        [Header("Energy")]
        [SerializeField] private Transform energyBar;
        [SerializeField] private EnergyCircleUI energyCirclePrefab;

        [Header("Display")]
        [SerializeField] private float circleScale = 3.5f;

        // Gap between circles in unscaled prefab pixels; the on-screen gap
        // is this multiplied by circleScale, so it grows with the circles.
        [SerializeField] private float circleSpacing = 5f;

        private HeroInstance hero;
        private readonly System.Collections.Generic.List<EnergyCircleUI> spawnedCircles = new();
        private ManaColor[] pendingPickOptions;

        private int lastMaxRed = -1;
        private int lastMaxYellow = -1;
        private int lastMaxBlue = -1;

        private int lastCurrentRed = -1;
        private int lastCurrentYellow = -1;
        private int lastCurrentBlue = -1;

        public HeroInstance BoundHero => hero;

        public void Bind(HeroInstance heroInstance)
        {
            hero = heroInstance;
            ForceFullRefresh();
        }

        public void Refresh()
        {
            if (hero == null)
            {
                ClearEnergyBar();
                return;
            }

            RefreshEnergyIfNeeded();
        }

        private void ForceFullRefresh()
        {
            lastMaxRed = -1;
            lastMaxYellow = -1;
            lastMaxBlue = -1;

            lastCurrentRed = -1;
            lastCurrentYellow = -1;
            lastCurrentBlue = -1;

            Refresh();
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
            ConfigureEnergyBarLayout();

            SpawnEnergyGroup(ManaColor.Red, hero.energyMax.red, hero.energyCurrent.red);
            SpawnEnergyGroup(ManaColor.Yellow, hero.energyMax.yellow, hero.energyCurrent.yellow);
            SpawnEnergyGroup(ManaColor.Blue, hero.energyMax.blue, hero.energyCurrent.blue);

            ApplyPendingPickState();
        }

        private void SpawnEnergyGroup(ManaColor color, int maxAmount, int currentAmount)
        {
            for (int i = 0; i < maxAmount; i++)
            {
                bool isFilled = i < currentAmount;
                SpawnEnergyCircle(color, isFilled);
            }
        }

        // The layout group measures rects, not localScale, so each circle's
        // LayoutElement must reserve the scaled footprint. Scale is kept on
        // the transform (the pulse animates it) without disturbing layout.
        private void ConfigureEnergyBarLayout()
        {
            HorizontalLayoutGroup layout = energyBar.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
                return;

            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = circleSpacing * circleScale;
        }

        private void SpawnEnergyCircle(ManaColor color, bool isFilled)
        {
            EnergyCircleUI circle = Instantiate(energyCirclePrefab, energyBar);
            circle.SetVisual(color, isFilled);
            circle.SetClickable(isFilled, HandleEnergyClicked);

            circle.transform.localScale = Vector3.one * circleScale;

            LayoutElement layoutElement = circle.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                if (layoutElement.preferredWidth > 0f)
                    layoutElement.preferredWidth *= circleScale;

                if (layoutElement.preferredHeight > 0f)
                    layoutElement.preferredHeight *= circleScale;
            }

            spawnedCircles.Add(circle);
        }

        // Colours the player may currently pick for a pending mana cost.
        // Pass null when no selection is pending.
        public void SetPendingPickOptions(ManaColor[] options)
        {
            pendingPickOptions = options;
            ApplyPendingPickState();
        }

        private void ApplyPendingPickState()
        {
            for (int i = 0; i < spawnedCircles.Count; i++)
            {
                EnergyCircleUI circle = spawnedCircles[i];
                if (circle == null)
                    continue;

                bool awaiting =
                    pendingPickOptions != null &&
                    circle.ClickIsFilled &&
                    System.Array.IndexOf(pendingPickOptions, circle.ClickColor) >= 0;

                circle.SetAwaitingPick(awaiting);
            }
        }

        private void HandleEnergyClicked(ManaColor color, bool isFilled)
        {
            if (!isFilled || hero == null)
                return;

            OnEnergyClicked?.Invoke(hero, color);
        }

        private void ClearEnergyBar()
        {
            spawnedCircles.Clear();

            if (energyBar == null)
                return;

            for (int i = energyBar.childCount - 1; i >= 0; i--)
            {
                Destroy(energyBar.GetChild(i).gameObject);
            }
        }
    }
}