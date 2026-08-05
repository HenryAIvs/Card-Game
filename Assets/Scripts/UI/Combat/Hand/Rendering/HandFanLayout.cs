using System.Collections.Generic;
using UnityEngine;

namespace UI.Combat
{
    public class HandFanLayout : MonoBehaviour
    {
        [Header("Fan Layout")]
        [SerializeField] private float cardSpacing = 140f;
        [SerializeField] private float maxRotation = 12f;
        [SerializeField] private float curveHeight = 45f;

        [Header("Hover")]
        [SerializeField] private float hoverLift = 55f;
        [SerializeField] private float hoverScale = 1.10f;
        [SerializeField] private float neighbourPush = 18f;

        [Header("Aiming")]
        [SerializeField] private float aimingLift = 85f;
        [SerializeField] private float aimingScale = 1.15f;

        [Header("Animation")]
        [SerializeField] private float moveLerpSpeed = 14f;
        [SerializeField] private float rotateLerpSpeed = 14f;
        [SerializeField] private float scaleLerpSpeed = 14f;

        private readonly List<RectTransform> orderedCards = new List<RectTransform>();

        private HandCardUI hoveredCard;
        private HandCardUI draggingCard;
        private HandCardUI aimingCard;

        private void LateUpdate()
        {
            RebuildOrderedCardList();
            ApplyLayout(immediate: false);
        }

        public void SetHoveredCard(HandCardUI card)
        {
            if (card == null)
                return;

            if (draggingCard != null)
                return;

            if (aimingCard != null && aimingCard != card)
                return;

            hoveredCard = card;
        }

        public void ClearHoveredCard(HandCardUI card)
        {
            if (hoveredCard == card)
                hoveredCard = null;
        }

        public void SetDraggingCard(HandCardUI card)
        {
            draggingCard = card;
            hoveredCard = null;
            aimingCard = null;
        }

        public void ClearDraggingCard(HandCardUI card)
        {
            if (draggingCard == card)
                draggingCard = null;
        }

        public void SetAimingCard(HandCardUI card)
        {
            aimingCard = card;
            hoveredCard = null;
            draggingCard = null;
        }

        public void ClearAimingCard(HandCardUI card)
        {
            if (aimingCard == card)
                aimingCard = null;
        }

        public void ForceRefresh()
        {
            RebuildOrderedCardList();
            ApplyLayout(immediate: true);
        }

        private void RebuildOrderedCardList()
        {
            orderedCards.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform rect = transform.GetChild(i) as RectTransform;
                if (rect == null)
                    continue;

                if (!rect.TryGetComponent(out HandCardUI _))
                    continue;

                orderedCards.Add(rect);
            }

            orderedCards.Sort(CompareByOriginalSiblingIndex);
        }

        private int CompareByOriginalSiblingIndex(RectTransform a, RectTransform b)
        {
            int aIndex = GetOriginalOrderIndex(a);
            int bIndex = GetOriginalOrderIndex(b);
            return aIndex.CompareTo(bIndex);
        }

        private int GetOriginalOrderIndex(RectTransform rect)
        {
            if (rect != null && rect.TryGetComponent(out HandCardVisualState visualState))
                return visualState.OriginalSiblingIndex;

            return rect != null ? rect.GetSiblingIndex() : 0;
        }

        private void ApplyLayout(bool immediate)
        {
            int count = orderedCards.Count;
            if (count == 0)
                return;

            int aimingIndex = GetTrackedCardIndex(ref aimingCard);
            int hoveredIndex = aimingIndex >= 0 || draggingCard != null ? -1 : GetTrackedCardIndex(ref hoveredCard);
            int focusIndex = aimingIndex >= 0 ? aimingIndex : hoveredIndex;

            for (int i = 0; i < count; i++)
            {
                RectTransform rect = orderedCards[i];
                if (rect == null)
                    continue;

                if (IsDraggingCard(rect))
                    continue;

                HandFanLayoutCalculator.LayoutPose pose =
                    BuildPose(i, count, hoveredIndex, aimingIndex, focusIndex);

                ApplyPose(rect, pose, immediate);
            }

            ApplyRenderOrder(hoveredIndex, aimingIndex);
        }

        private HandFanLayoutCalculator.LayoutPose BuildPose(
            int index,
            int count,
            int hoveredIndex,
            int aimingIndex,
            int focusIndex
        )
        {
            return HandFanLayoutCalculator.BuildPose(
                index,
                count,
                hoveredIndex,
                aimingIndex,
                focusIndex,
                cardSpacing,
                maxRotation,
                curveHeight,
                hoverLift,
                hoverScale,
                aimingLift,
                aimingScale,
                neighbourPush
            );
        }

        private bool IsDraggingCard(RectTransform rect)
        {
            if (rect == null || draggingCard == null)
                return false;

            return rect.TryGetComponent(out HandCardUI card) && card == draggingCard;
        }

        private void ApplyPose(
            RectTransform rect,
            HandFanLayoutCalculator.LayoutPose pose,
            bool immediate
        )
        {
            if (immediate)
            {
                rect.anchoredPosition = pose.Position;
                rect.localRotation = pose.Rotation;
                rect.localScale = pose.Scale;
                return;
            }

            rect.anchoredPosition = Vector2.Lerp(
                rect.anchoredPosition,
                pose.Position,
                Time.deltaTime * moveLerpSpeed
            );

            rect.localRotation = Quaternion.Lerp(
                rect.localRotation,
                pose.Rotation,
                Time.deltaTime * rotateLerpSpeed
            );

            rect.localScale = Vector3.Lerp(
                rect.localScale,
                pose.Scale,
                Time.deltaTime * scaleLerpSpeed
            );
        }

        private void ApplyRenderOrder(int hoveredIndex, int aimingIndex)
        {
            for (int i = 0; i < orderedCards.Count; i++)
            {
                RectTransform rect = orderedCards[i];
                if (rect == null)
                    continue;

                rect.SetSiblingIndex(i);
            }

            if (draggingCard != null)
                return;

            int topIndex = aimingIndex >= 0 ? aimingIndex : hoveredIndex;
            if (topIndex < 0 || topIndex >= orderedCards.Count)
                return;

            RectTransform topRect = orderedCards[topIndex];
            if (topRect != null)
                topRect.SetAsLastSibling();
        }

        private int GetTrackedCardIndex(ref HandCardUI trackedCard)
        {
            if (trackedCard == null)
                return -1;

            for (int i = 0; i < orderedCards.Count; i++)
            {
                RectTransform rect = orderedCards[i];
                if (rect == null)
                    continue;

                if (rect.TryGetComponent(out HandCardUI card) && card == trackedCard)
                    return i;
            }

            trackedCard = null;
            return -1;
        }
    }
}