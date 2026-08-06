using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Combat.Cards;
using Combat.Data.Effects;

namespace UI.Combat
{
    // Modal overlay for the Scry effect: shows the top X cards of the hero's
    // draw pile (leftmost = drawn first) and lets the player drag them into
    // a new order. Confirm writes the order back to the deck.
    //
    // The panel is built entirely in code and the card visuals reuse the
    // hand's card prefab, so nothing needs wiring in the scene beyond the
    // host object CombatHudUI spawns.
    public class ScryOverlayUI : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private float cardSpacing = 250f;
        [SerializeField] private float cardScale = 1f;
        [SerializeField] private float moveLerpSpeed = 14f;

        private Canvas rootCanvas;
        private HandCardUI cardPrefab;

        private readonly Queue<ScryRequest> pendingRequests = new Queue<ScryRequest>();
        private ScryRequest activeRequest;

        private RectTransform overlayRoot;
        private RectTransform cardRow;
        private readonly List<CardInstance> order = new List<CardInstance>();
        private readonly Dictionary<CardInstance, RectTransform> cardVisuals = new Dictionary<CardInstance, RectTransform>();
        private CardInstance draggingCard;

        public bool IsOpen => overlayRoot != null;

        private void OnEnable()
        {
            ScryEffectSO.OnScryRequested += HandleScryRequested;
        }

        private void OnDisable()
        {
            ScryEffectSO.OnScryRequested -= HandleScryRequested;
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            for (int i = 0; i < order.Count; i++)
            {
                CardInstance card = order[i];

                if (card == draggingCard)
                    continue;

                if (!cardVisuals.TryGetValue(card, out RectTransform rect) || rect == null)
                    continue;

                rect.anchoredPosition = Vector2.Lerp(
                    rect.anchoredPosition,
                    SlotPosition(i),
                    Time.deltaTime * moveLerpSpeed
                );
            }
        }

        private void HandleScryRequested(ScryRequest request)
        {
            if (request == null || request.hero == null)
                return;

            pendingRequests.Enqueue(request);
            TryOpenNext();
        }

        private void TryOpenNext()
        {
            if (IsOpen)
                return;

            while (pendingRequests.Count > 0)
            {
                ScryRequest request = pendingRequests.Dequeue();
                List<CardInstance> top = request.hero.deck.PeekTop(request.amount);

                if (top.Count <= 1)
                    continue;

                activeRequest = request;
                BuildOverlay(top);
                return;
            }
        }

        private void BuildOverlay(List<CardInstance> cards)
        {
            if (!EnsureSceneReferences())
            {
                Debug.LogWarning("ScryOverlayUI: missing canvas or card prefab, applying no reorder.");
                activeRequest = null;
                return;
            }

            overlayRoot = CreateRect("ScryOverlay", rootCanvas.transform);
            overlayRoot.anchorMin = Vector2.zero;
            overlayRoot.anchorMax = Vector2.one;
            overlayRoot.sizeDelta = Vector2.zero;
            overlayRoot.SetAsLastSibling();

            // Dim backdrop; also swallows clicks so the combat UI is modal.
            Image dim = overlayRoot.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.7f);
            dim.raycastTarget = true;

            TextMeshProUGUI title = CreateText(
                "Title",
                overlayRoot,
                $"Scry {cards.Count}",
                42f
            );
            title.rectTransform.anchoredPosition = new Vector2(0f, 320f);
            title.rectTransform.sizeDelta = new Vector2(900f, 60f);

            TextMeshProUGUI hint = CreateText(
                "Hint",
                overlayRoot,
                "Drag to reorder - the leftmost card is drawn first",
                24f
            );
            hint.rectTransform.anchoredPosition = new Vector2(0f, 260f);
            hint.rectTransform.sizeDelta = new Vector2(900f, 40f);

            cardRow = CreateRect("CardRow", overlayRoot);
            cardRow.anchoredPosition = new Vector2(0f, 20f);

            order.Clear();
            cardVisuals.Clear();
            draggingCard = null;

            for (int i = 0; i < cards.Count; i++)
            {
                CardInstance card = cards[i];
                if (card == null)
                    continue;

                HandCardUI cardUI = Instantiate(cardPrefab, cardRow);

                // The hand's drag/play behaviour must not run inside the
                // overlay; scry cards only reorder.
                HandCardDragHandler handDrag = cardUI.GetComponent<HandCardDragHandler>();
                if (handDrag != null)
                    Destroy(handDrag);

                cardUI.Bind(card);
                cardUI.transform.localScale = Vector3.one * cardScale;

                ScryCardDragHandler drag = cardUI.gameObject.AddComponent<ScryCardDragHandler>();
                drag.Init(this, card);

                RectTransform rect = (RectTransform)cardUI.transform;
                order.Add(card);
                cardVisuals[card] = rect;
                rect.anchoredPosition = SlotPosition(order.Count - 1);
            }

            BuildConfirmButton();
        }

        private void BuildConfirmButton()
        {
            RectTransform buttonRect = CreateRect("ConfirmButton", overlayRoot);
            buttonRect.anchoredPosition = new Vector2(0f, -280f);
            buttonRect.sizeDelta = new Vector2(240f, 60f);

            Image background = buttonRect.gameObject.AddComponent<Image>();
            background.color = new Color(0.85f, 0.85f, 0.85f, 1f);

            Button button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(Confirm);

            TextMeshProUGUI label = CreateText("Label", buttonRect, "Confirm", 28f);
            label.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.sizeDelta = Vector2.zero;
            label.rectTransform.anchoredPosition = Vector2.zero;
        }

        private void Confirm()
        {
            if (activeRequest != null && activeRequest.hero != null && order.Count > 0)
            {
                activeRequest.hero.deck.SetTopOrder(new List<CardInstance>(order));
                Debug.Log($"[Scry] {activeRequest.hero.id} confirmed new top {order.Count} order.");
            }

            CloseOverlay();
            TryOpenNext();
        }

        private void CloseOverlay()
        {
            if (overlayRoot != null)
                Destroy(overlayRoot.gameObject);

            overlayRoot = null;
            cardRow = null;
            order.Clear();
            cardVisuals.Clear();
            draggingCard = null;
            activeRequest = null;
        }

        public void BeginCardDrag(CardInstance card)
        {
            if (!cardVisuals.TryGetValue(card, out RectTransform rect) || rect == null)
                return;

            draggingCard = card;
            rect.SetAsLastSibling();
        }

        public void DragCard(CardInstance card, Vector2 screenDelta)
        {
            if (card != draggingCard)
                return;

            if (!cardVisuals.TryGetValue(card, out RectTransform rect) || rect == null)
                return;

            float scaleFactor = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
            if (scaleFactor <= 0f)
                scaleFactor = 1f;

            rect.anchoredPosition += screenDelta / scaleFactor;

            int currentIndex = order.IndexOf(card);
            int desiredIndex = Mathf.Clamp(
                Mathf.RoundToInt(rect.anchoredPosition.x / cardSpacing + (order.Count - 1) * 0.5f),
                0,
                order.Count - 1
            );

            if (currentIndex >= 0 && desiredIndex != currentIndex)
            {
                order.RemoveAt(currentIndex);
                order.Insert(desiredIndex, card);
            }
        }

        public void EndCardDrag(CardInstance card)
        {
            if (draggingCard == card)
                draggingCard = null;
        }

        private Vector2 SlotPosition(int index)
        {
            float centered = index - (order.Count - 1) * 0.5f;
            return new Vector2(centered * cardSpacing, 0f);
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

            return rootCanvas != null && cardPrefab != null;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;

            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize)
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

    // Attached at runtime to each card visual inside the scry overlay.
    public class ScryCardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private ScryOverlayUI owner;
        private CardInstance card;

        public void Init(ScryOverlayUI overlayOwner, CardInstance boundCard)
        {
            owner = overlayOwner;
            card = boundCard;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (owner != null)
                owner.BeginCardDrag(card);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (owner != null)
                owner.DragCard(card, eventData.delta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (owner != null)
                owner.EndCardDrag(card);
        }
    }
}
