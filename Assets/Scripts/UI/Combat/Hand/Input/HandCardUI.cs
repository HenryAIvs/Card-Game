using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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

        [Header("Playable Visuals")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float playableAlpha = 1f;
        [SerializeField] private float unplayableAlpha = 0.65f;

        private CardInstance boundCard;
        private HandFanLayout cachedFanLayout;
        private bool isPlayable = true;
        private bool forceHidden = false;

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
                rulesText.text = cardData != null && !string.IsNullOrWhiteSpace(cardData.rulesText)
                    ? cardData.rulesText
                    : "";
            }

            RefreshCostVisuals(cardData);
        }

        private void RefreshCostVisuals(HeroCardSO cardData)
        {
            if (costRow == null || costCirclePrefab == null)
                return;

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