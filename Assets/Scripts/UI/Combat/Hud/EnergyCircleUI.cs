using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Combat.Resources.Mana;

namespace UI.Combat
{
    public class EnergyCircleUI : MonoBehaviour
    {
        [SerializeField] private Button clickButton;
        [SerializeField] private Image outerRing;
        [SerializeField] private Image innerFillLeft;
        [SerializeField] private Image innerFillRight;

        private static readonly Color RedFill = new Color(0.85f, 0.2f, 0.2f);
        private static readonly Color YellowFill = new Color(0.95f, 0.85f, 0.2f);
        private static readonly Color BlueFill = new Color(0.25f, 0.5f, 0.95f);
        private static readonly Color EmptyFill = new Color(0.45f, 0.45f, 0.45f);
        private static readonly Color GenericFill = new Color(0.7f, 0.7f, 0.7f);
        private static readonly Color SelectableRing = new Color(1f, 1f, 1f);
        private static readonly Color NormalRing = Color.black;

        [Header("Pick Flash")]
        [SerializeField] private float pulseSpeed = 7f;
        [SerializeField] private float pulseScaleAmount = 0.25f;
        [SerializeField] private float pulseBrightenAmount = 0.45f;

        private ManaColor clickColor;
        private bool clickIsFilled;
        private bool isClickable;
        private bool isAwaitingPick;
        private Vector3 baseScale = Vector3.one;
        private Color baseFillColor = Color.white;
        private Action<ManaColor, bool> onClicked;

        public ManaColor ClickColor => clickColor;
        public bool ClickIsFilled => clickIsFilled;

        private void Awake()
        {
            if (clickButton != null)
            {
                clickButton.onClick.RemoveAllListeners();
                clickButton.onClick.AddListener(HandleClicked);
            }
        }

        private void Update()
        {
            if (!isAwaitingPick)
                return;

            float t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed);

            // Brightness pulse of the circle's own colour - no hue change.
            SetBothHalves(Color.Lerp(baseFillColor, Color.white, pulseBrightenAmount * t));
            transform.localScale = baseScale * (1f + pulseScaleAmount * t);
        }

        // Flashes the circle while the player must pick a mana colour.
        public void SetAwaitingPick(bool awaiting)
        {
            if (isAwaitingPick == awaiting)
                return;

            if (awaiting)
                baseScale = transform.localScale;

            isAwaitingPick = awaiting;

            if (!awaiting)
            {
                transform.localScale = baseScale;
                SetBothHalves(baseFillColor);
            }
        }

        public void SetVisual(ManaColor color, bool isFilled)
        {
            clickColor = color;
            clickIsFilled = isFilled;

            if (outerRing != null)
                outerRing.color = NormalRing;

            Color fillColor = isFilled ? GetManaColor(color) : EmptyFill;
            baseFillColor = fillColor;
            SetBothHalves(fillColor);
        }

        public void SetCostVisual(List<ManaColor> options)
        {
            if (outerRing != null)
                outerRing.color = NormalRing;

            if (options == null || options.Count == 0)
            {
                SetBothHalves(GenericFill);
                return;
            }

            if (options.Count == 1)
            {
                SetBothHalves(GetManaColor(options[0]));
                return;
            }

            if (options.Count == 2)
            {
                if (innerFillLeft != null)
                    innerFillLeft.color = GetManaColor(options[0]);

                if (innerFillRight != null)
                    innerFillRight.color = GetManaColor(options[1]);

                return;
            }

            SetBothHalves(GenericFill);
        }

        public void SetClickable(bool clickable, Action<ManaColor, bool> clickedHandler)
        {
            isClickable = clickable;
            onClicked = clickedHandler;

            if (outerRing != null)
                outerRing.color = clickable ? SelectableRing : NormalRing;

            if (clickButton != null)
                clickButton.interactable = clickable;
        }

        private void HandleClicked()
        {
            if (!isClickable)
                return;

            onClicked?.Invoke(clickColor, clickIsFilled);
        }

        private void SetBothHalves(Color color)
        {
            if (innerFillLeft != null)
                innerFillLeft.color = color;

            if (innerFillRight != null)
                innerFillRight.color = color;
        }

        private Color GetManaColor(ManaColor color)
        {
            switch (color)
            {
                case ManaColor.Red:
                    return RedFill;
                case ManaColor.Yellow:
                    return YellowFill;
                case ManaColor.Blue:
                    return BlueFill;
                default:
                    return Color.white;
            }
        }
    }
}