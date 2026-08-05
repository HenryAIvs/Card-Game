using System.Collections;
using UnityEngine;
using Combat.Cards;

namespace UI.Combat
{
    public class HeroHandDrawAnimator
    {
        private readonly HeroHandUI owner;

        public HeroHandDrawAnimator(HeroHandUI owner)
        {
            this.owner = owner;
        }

        public IEnumerator PlayDrawAnimation(CardInstance drawnCard)
        {
            if (drawnCard == null || owner.BoundHero == null)
                yield break;

            owner.BeginVisualRefreshLock();

            try
            {
                Canvas rootCanvas = owner.GetRootCanvas();
                Transform cardContainer = owner.GetCardContainer();
                RectTransform containerRect = cardContainer as RectTransform;
                HandFanLayout fanLayout = cardContainer != null
                    ? cardContainer.GetComponent<HandFanLayout>()
                    : null;

                if (rootCanvas == null || containerRect == null || fanLayout == null)
                {
                    owner.RefreshHand(true);
                    owner.RefreshPlayableStates();
                    yield break;
                }

                // Build the real hand with the newly drawn card included.
                // This lets the rest of the hand start spreading immediately.
                owner.RefreshHand(immediate: false);
                owner.RefreshPlayableStates();

                HandCardUI placeholderCardUI = owner.FindCardUI(drawnCard);
                if (placeholderCardUI == null)
                {
                    owner.RefreshHand(true);
                    owner.RefreshPlayableStates();
                    yield break;
                }

                // Hide the real card in hand, but leave it in layout so it reserves space.
                placeholderCardUI.SetForceHidden(true);

                HandCardUI animatedCardUI = CreateAnimatedCard(drawnCard, rootCanvas);
                if (animatedCardUI == null)
                {
                    placeholderCardUI.SetForceHidden(false);
                    owner.RefreshHand(true);
                    owner.RefreshPlayableStates();
                    yield break;
                }

                yield return AnimateCardToPlaceholder(animatedCardUI, placeholderCardUI, rootCanvas);

                Object.Destroy(animatedCardUI.gameObject);

                placeholderCardUI.SetForceHidden(false);
                owner.RefreshHand(immediate: true);
                owner.RefreshPlayableStates();
            }
            finally
            {
                owner.EndVisualRefreshLock();
            }
        }

        private HandCardUI CreateAnimatedCard(CardInstance drawnCard, Canvas rootCanvas)
        {
            HandCardUI prefab = owner.GetCardPrefab();
            if (prefab == null || rootCanvas == null)
                return null;

            HandCardUI cardUI = Object.Instantiate(prefab, rootCanvas.transform);
            cardUI.name = prefab.name + "_AnimatedDraw";
            cardUI.Bind(drawnCard);
            cardUI.SetForceHidden(false);

            CanvasGroup group = cardUI.GetComponent<CanvasGroup>();
            if (group == null)
                group = cardUI.gameObject.AddComponent<CanvasGroup>();

            group.blocksRaycasts = false;
            group.interactable = false;
            group.alpha = 1f;

            HandCardDragHandler dragHandler = cardUI.GetComponent<HandCardDragHandler>();
            if (dragHandler != null)
                dragHandler.enabled = false;

            return cardUI;
        }

        private IEnumerator AnimateCardToPlaceholder(
            HandCardUI animatedCardUI,
            HandCardUI placeholderCardUI,
            Canvas rootCanvas
        )
        {
            RectTransform animatedRect = animatedCardUI.transform as RectTransform;
            RectTransform placeholderRect = placeholderCardUI.transform as RectTransform;
            RectTransform canvasRect = rootCanvas.transform as RectTransform;

            if (animatedRect == null || placeholderRect == null || canvasRect == null)
                yield break;

            Vector3 endWorldPos = placeholderRect.position;
            Quaternion endWorldRot = placeholderRect.rotation;
            Vector3 endScale = placeholderRect.lossyScale;

            Vector3 endCanvasLocal = canvasRect.InverseTransformPoint(endWorldPos);
            Vector3 startCanvasLocal = new Vector3(
                -canvasRect.rect.width * 0.5f - owner.DrawStartOffsetX,
                endCanvasLocal.y + owner.DrawStartOffsetY,
                0f
            );

            Vector3 startWorldPos = canvasRect.TransformPoint(startCanvasLocal);
            Quaternion startWorldRot = endWorldRot * Quaternion.Euler(0f, 0f, owner.DrawStartRotationZ);

            animatedRect.position = startWorldPos;
            animatedRect.rotation = startWorldRot;
            animatedRect.localScale = Vector3.one * owner.DrawStartScale;

            float elapsed = 0f;

            while (elapsed < owner.DrawAnimDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / owner.DrawAnimDuration);
                float eased = EaseOutCubic(t);

                // Track the placeholder every frame so the animated card follows
                // the hand while the fan layout spreads.
                endWorldPos = placeholderRect.position;
                endWorldRot = placeholderRect.rotation;

                animatedRect.position = Vector3.LerpUnclamped(startWorldPos, endWorldPos, eased);
                animatedRect.rotation = Quaternion.SlerpUnclamped(startWorldRot, endWorldRot, eased);
                animatedRect.localScale = Vector3.LerpUnclamped(
                    Vector3.one * owner.DrawStartScale,
                    Vector3.one,
                    eased
                );

                yield return null;
            }

            animatedRect.position = placeholderRect.position;
            animatedRect.rotation = placeholderRect.rotation;
            animatedRect.localScale = Vector3.one;
        }

        private float EaseOutCubic(float t)
        {
            float inv = 1f - t;
            return 1f - (inv * inv * inv);
        }
    }
}