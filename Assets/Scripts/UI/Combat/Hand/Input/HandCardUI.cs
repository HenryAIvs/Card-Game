using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Combat.Cards;
using Combat.Data.Cards;
using Combat.Data.Mana;

namespace UI.Combat
{
    public class HandCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text rulesText;
        [SerializeField] private RectTransform arrowOrigin;

        [Header("Cost UI")]
        [SerializeField] private Transform costRow;
        [SerializeField] private EnergyCircleUI costCirclePrefab;
        [SerializeField] private float costCircleSpacing = 14f;

        [Header("Playable Visuals")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float playableAlpha = 1f;
        [SerializeField] private float unplayableAlpha = 0.65f;

        [Header("Parity Highlight")]
        [SerializeField] private Color parityGlowColor = new Color(1f, 0.88f, 0.45f, 1f);

        private CardInstance boundCard;
        private HandFanLayout cachedFanLayout;
        private bool isPlayable = true;
        private bool forceHidden = false;
        private string baseRulesText = "";
        private int lastParityHandCount = -1;

        public CardInstance BoundCard => boundCard;
        public bool IsPlayable => isPlayable;
        public RectTransform ArrowOrigin => arrowOrigin != null ? arrowOrigin : transform as RectTransform;

        private void Awake()
        {
            CacheReferences();
        }

        public void Bind(CardInstance cardInstance)
        {
            boundCard = cardInstance;
            CacheReferences();

            RefreshVisuals();
            RefreshPlayableVisuals();
        }

        public void SetPlayableState(bool playable)
        {
            isPlayable = playable;
            RefreshPlayableVisuals();
        }

        public void SetForceHidden(bool hidden)
        {
            forceHidden = hidden;
            RefreshPlayableVisuals();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            CacheReferences();
            cachedFanLayout?.SetHoveredCard(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CacheReferences();
            cachedFanLayout?.ClearHoveredCard(this);
        }

        private void CacheReferences()
        {
            if (cachedFanLayout == null)
                cachedFanLayout = GetComponentInParent<HandFanLayout>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        private void RefreshPlayableVisuals()
        {
            if (canvasGroup == null)
                return;

            float alpha = isPlayable ? playableAlpha : unplayableAlpha;

            if (forceHidden)
                alpha = 0f;

            canvasGroup.alpha = alpha;
            canvasGroup.blocksRaycasts = !forceHidden;
            canvasGroup.interactable = !forceHidden;
        }

        private void RefreshVisuals()
        {
            HeroCardSO cardData = boundCard != null ? boundCard.card : null;

            if (nameText != null)
            {
                nameText.text = cardData != null && !string.IsNullOrWhiteSpace(cardData.displayName)
                    ? cardData.displayName
                    : "Card";
            }

            if (rulesText != null)
            {
                baseRulesText = cardData != null && !string.IsNullOrWhiteSpace(cardData.rulesText)
                    ? cardData.rulesText
                    : "";

                rulesText.text = baseRulesText;
                lastParityHandCount = -1;
            }

            RefreshCostVisuals(cardData);
        }

        // Lights up the "Left:"/"Right:" clause whose parity condition the
        // current hand size satisfies. Parity resolves before the played card
        // leaves the hand, so the raw hand count is the right predictor.
        public void RefreshParityHighlight(int handCount)
        {
            if (rulesText == null || handCount == lastParityHandCount)
                return;

            lastParityHandCount = handCount;

            if (string.IsNullOrEmpty(baseRulesText))
                return;

            bool isOdd = (handCount % 2) == 1;

            string highlighted = baseRulesText;
            highlighted = HighlightClause(highlighted, "Left:", isOdd);
            highlighted = HighlightClause(highlighted, "Right:", !isOdd);

            rulesText.text = highlighted;
        }

        // Cards write the parity keyword on its own line with the effect
        // indented on the next line, so the highlight spans both lines.
        private string HighlightClause(string text, string keyword, bool active)
        {
            if (!active)
                return text;

            int start = text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            if (start < 0)
                return text;

            int end = text.IndexOf('\n', start);
            if (end >= 0)
                end = text.IndexOf('\n', end + 1);
            if (end < 0)
                end = text.Length;

            string hex = ColorUtility.ToHtmlStringRGBA(parityGlowColor);

            return string.Concat(
                text.Substring(0, start),
                "<color=#", hex, "><b>",
                text.Substring(start, end - start),
                "</b></color>",
                text.Substring(end)
            );
        }

        // The prefab's CostRow has no layout group, so without this every
        // cost circle spawns on the same spot and multi-colour costs read
        // as a single circle.
        private void ConfigureCostRowLayout()
        {
            HorizontalLayoutGroup layout = costRow.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
                layout = costRow.gameObject.AddComponent<HorizontalLayoutGroup>();

            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = costCircleSpacing;
        }

        private void RefreshCostVisuals(HeroCardSO cardData)
        {
            if (costRow == null || costCirclePrefab == null)
                return;

            ConfigureCostRowLayout();
            ClearCostRow();

            if (cardData == null || cardData.cost == null || cardData.cost.costs == null)
                return;

            for (int i = 0; i < cardData.cost.costs.Count; i++)
            {
                ChoiceCostData chunk = cardData.cost.costs[i];
                if (chunk == null || chunk.amount <= 0)
                    continue;

                if (chunk.options == null || chunk.options.Count == 0)
                    continue;

                for (int pip = 0; pip < chunk.amount; pip++)
                    SpawnCostCircle(chunk);
            }
        }

        private void SpawnCostCircle(ChoiceCostData chunk)
        {
            EnergyCircleUI circle = Instantiate(costCirclePrefab, costRow);
            circle.SetCostVisual(chunk.options);
        }

        private void ClearCostRow()
        {
            for (int i = costRow.childCount - 1; i >= 0; i--)
                Destroy(costRow.GetChild(i).gameObject);
        }
    }
}