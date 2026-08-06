using System.Collections;
using TMPro;
using UnityEngine;
using Combat.Cards;
using Combat.Data.Cards;

namespace UI.Combat
{
    // Presents a slow card as it resolves: the actual card visual (reusing
    // the hand's card prefab) with a caption above it, faded in and out.
    // Built in code; CombatRunner's slow resolve sequence spawns it.
    public class SlowCardPresenterUI : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float holdTime = 1.2f;
        [SerializeField] private float fadeTime = 0.18f;

        [Header("Layout")]
        [SerializeField] private float cardScale = 1.25f;

        private Canvas rootCanvas;
        private HandCardUI cardPrefab;

        public IEnumerator ShowSlowCard(SlowCardPlay play)
        {
            if (play == null || !EnsureSceneReferences())
                yield break;

            RectTransform holder = CreateRect("SlowCardDisplay", rootCanvas.transform);
            holder.anchoredPosition = Vector2.zero;
            holder.SetAsLastSibling();

            CanvasGroup group = holder.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            string heroName = play.source != null ? play.source.id : "Hero";
            TextMeshProUGUI caption = CreateText("Caption", holder, $"{heroName}'s slow card resolves", 30f);
            caption.rectTransform.anchoredPosition = new Vector2(0f, 240f);
            caption.rectTransform.sizeDelta = new Vector2(900f, 44f);

            if (play.card != null && cardPrefab != null)
            {
                HandCardUI cardUI = Instantiate(cardPrefab, holder);

                HandCardDragHandler dragHandler = cardUI.GetComponent<HandCardDragHandler>();
                if (dragHandler != null)
                    Destroy(dragHandler);

                cardUI.Bind(new CardInstance(play.card));
                cardUI.transform.localScale = Vector3.one * cardScale;

                RectTransform cardRect = (RectTransform)cardUI.transform;
                cardRect.anchoredPosition = new Vector2(0f, -20f);
            }
            else
            {
                TextMeshProUGUI fallback = CreateText("CardName", holder, play.cardName, 44f);
                fallback.rectTransform.anchoredPosition = new Vector2(0f, 0f);
                fallback.rectTransform.sizeDelta = new Vector2(900f, 70f);
            }

            yield return Fade(group, 0f, 1f);
            yield return new WaitForSeconds(holdTime);
            yield return Fade(group, 1f, 0f);

            Destroy(holder.gameObject);
        }

        private IEnumerator Fade(CanvasGroup group, float from, float to)
        {
            if (fadeTime <= 0f)
            {
                group.alpha = to;
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / fadeTime));
                yield return null;
            }

            group.alpha = to;
        }

        private bool EnsureSceneReferences()
        {
            if (rootCanvas == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                    rootCanvas = canvas.rootCanvas;
            }

            if (cardPrefab == null)
            {
                HeroHandUI handUI = FindFirstObjectByType<HeroHandUI>();
                if (handUI != null)
                    cardPrefab = handUI.GetCardPrefab();
            }

            return rootCanvas != null;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;

            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize)
        {
            RectTransform rect = CreateRect(name, parent);

            TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            return tmp;
        }
    }
}
