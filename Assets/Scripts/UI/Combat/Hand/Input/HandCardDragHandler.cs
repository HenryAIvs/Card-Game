using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Combat
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class HandCardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private HandCardUI cardUI;
        [SerializeField] private HeroHandUI heroHandUI;

        [Header("Drag")]
        [SerializeField] private float dragPlayOffsetY = 180f;
        [SerializeField] private float dragScale = 1.1f;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas rootCanvas;
        private HandFanLayout handFanLayout;

        private Transform originalParent;
        private int originalSiblingIndex;
        private Vector2 originalAnchoredPosition;
        private Vector3 originalScale;
        private Quaternion originalRotation;

        private float computedPlayThresholdY;

        private bool isDragging = false;
        private bool isTargetingDrag = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            if (cardUI == null)
                cardUI = GetComponent<HandCardUI>();

            if (heroHandUI == null)
                heroHandUI = GetComponentInParent<HeroHandUI>();

            handFanLayout = GetComponentInParent<HandFanLayout>();
            rootCanvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = false;
            isTargetingDrag = false;

            if (cardUI == null || !cardUI.IsPlayable)
                return;

            if (heroHandUI == null)
                return;

            if (heroHandUI.IsInteractionLocked)
                return;

            if (!heroHandUI.CanBeginDragPlay(cardUI.BoundCard))
                return;

            if (heroHandUI.TryBeginTargeting(cardUI.BoundCard, cardUI))
            {
                isTargetingDrag = true;

                if (handFanLayout == null)
                    handFanLayout = GetComponentInParent<HandFanLayout>();

                if (handFanLayout != null)
                    handFanLayout.SetDraggingCard(cardUI);

                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 1f;

                return;
            }

            if (rootCanvas == null)
                rootCanvas = GetComponentInParent<Canvas>();

            if (rootCanvas == null)
                return;

            isDragging = true;

            originalParent = rectTransform.parent;
            originalSiblingIndex = rectTransform.GetSiblingIndex();
            originalAnchoredPosition = rectTransform.anchoredPosition;
            originalScale = rectTransform.localScale;
            originalRotation = rectTransform.localRotation;

            computedPlayThresholdY = originalAnchoredPosition.y + dragPlayOffsetY;

            if (handFanLayout == null)
                handFanLayout = GetComponentInParent<HandFanLayout>();

            if (handFanLayout != null)
                handFanLayout.SetDraggingCard(cardUI);

            rectTransform.SetParent(rootCanvas.transform, true);
            rectTransform.SetAsLastSibling();
            rectTransform.localScale = Vector3.one * dragScale;
            rectTransform.localRotation = Quaternion.identity;

            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 1f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isTargetingDrag)
                return;

            if (!isDragging)
                return;

            if (rootCanvas == null)
                return;

            float scaleFactor = rootCanvas.scaleFactor;
            if (scaleFactor <= 0f)
                scaleFactor = 1f;

            rectTransform.anchoredPosition += eventData.delta / scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isTargetingDrag)
            {
                isTargetingDrag = false;
                return;
            }

            if (!isDragging)
                return;

            isDragging = false;

            bool shouldAttemptPlay = rectTransform.anchoredPosition.y >= computedPlayThresholdY;
            bool played = false;

            if (shouldAttemptPlay && heroHandUI != null && cardUI != null && !heroHandUI.IsInteractionLocked)
            {
                played = heroHandUI.TryPlayCardFromDrag(cardUI.BoundCard);
            }

            canvasGroup.blocksRaycasts = true;

            if (handFanLayout != null)
                handFanLayout.ClearDraggingCard(cardUI);

            if (played)
            {
                Destroy(gameObject);
                return;
            }

            ReturnToHand();
        }

        private void ReturnToHand()
        {
            if (originalParent != null)
                rectTransform.SetParent(originalParent, false);

            rectTransform.SetSiblingIndex(originalSiblingIndex);
            rectTransform.anchoredPosition = originalAnchoredPosition;
            rectTransform.localScale = originalScale;
            rectTransform.localRotation = originalRotation;

            if (handFanLayout == null)
                handFanLayout = GetComponentInParent<HandFanLayout>();

            if (handFanLayout != null)
                handFanLayout.ForceRefresh();
        }
    }
}