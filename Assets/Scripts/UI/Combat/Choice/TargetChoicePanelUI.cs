using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Combat.Data.Effects;

namespace UI.Combat
{
    // Modal panel shown when an effect lets its TARGET choose which option
    // applies to them (e.g. the goblin's Scalp: take Exhaustion or Hit 2).
    // Built entirely in code; CombatHudUI spawns the host object.
    public class TargetChoicePanelUI : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private float buttonWidth = 460f;
        [SerializeField] private float buttonHeight = 64f;
        [SerializeField] private float buttonSpacing = 80f;

        private Canvas rootCanvas;

        private readonly Queue<TargetChoiceRequest> pendingQueue = new Queue<TargetChoiceRequest>();
        private TargetChoiceRequest activeRequest;
        private RectTransform overlayRoot;

        private void OnEnable()
        {
            TargetChoiceEffectSO.OnChoiceRequested += HandleChoiceRequested;
        }

        private void OnDisable()
        {
            TargetChoiceEffectSO.OnChoiceRequested -= HandleChoiceRequested;
        }

        private void HandleChoiceRequested(TargetChoiceRequest request)
        {
            if (request == null)
                return;

            pendingQueue.Enqueue(request);
            TryShowNext();
        }

        private void TryShowNext()
        {
            if (activeRequest != null || pendingQueue.Count == 0)
                return;

            activeRequest = pendingQueue.Dequeue();
            BuildOverlay();
        }

        private void BuildOverlay()
        {
            if (!EnsureCanvas())
            {
                // No UI available; fall back to the first option so the game
                // never hangs on an unresolvable choice.
                TargetChoiceRequest orphan = activeRequest;
                activeRequest = null;
                orphan.ResolvePick(0);
                return;
            }

            overlayRoot = CreateRect("TargetChoiceOverlay", rootCanvas.transform);
            overlayRoot.anchorMin = Vector2.zero;
            overlayRoot.anchorMax = Vector2.one;
            overlayRoot.sizeDelta = Vector2.zero;
            overlayRoot.SetAsLastSibling();

            Image dim = overlayRoot.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.7f);
            dim.raycastTarget = true;

            string sourceName = activeRequest.source != null ? activeRequest.source.id : "Enemy";
            string targetName = activeRequest.target != null ? activeRequest.target.id : "Hero";

            TextMeshProUGUI title = CreateText("Title", overlayRoot, $"{sourceName}'s effect", 34f);
            title.rectTransform.anchoredPosition = new Vector2(0f, 180f);
            title.rectTransform.sizeDelta = new Vector2(900f, 50f);

            TextMeshProUGUI subtitle = CreateText("Subtitle", overlayRoot, $"{targetName} chooses one:", 24f);
            subtitle.rectTransform.anchoredPosition = new Vector2(0f, 130f);
            subtitle.rectTransform.sizeDelta = new Vector2(900f, 36f);
            subtitle.color = new Color(0.85f, 0.85f, 0.85f, 1f);

            List<EffectSO> options = activeRequest.effect.options;

            for (int i = 0; i < options.Count; i++)
            {
                if (options[i] == null)
                    continue;

                BuildOptionButton(i, EffectDescriber.Describe(options[i]));
            }
        }

        private void BuildOptionButton(int optionIndex, string label)
        {
            RectTransform buttonRect = CreateRect($"Option{optionIndex}", overlayRoot);
            buttonRect.anchoredPosition = new Vector2(0f, 50f - optionIndex * buttonSpacing);
            buttonRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);

            Image background = buttonRect.gameObject.AddComponent<Image>();
            background.color = new Color(0.85f, 0.85f, 0.85f, 1f);

            Button button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;

            int captured = optionIndex;
            button.onClick.AddListener(() => Choose(captured));

            TextMeshProUGUI text = CreateText("Label", buttonRect, label, 26f);
            text.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.sizeDelta = Vector2.zero;
            text.rectTransform.anchoredPosition = Vector2.zero;
        }

        private void Choose(int optionIndex)
        {
            TargetChoiceRequest request = activeRequest;

            CloseOverlay();

            // Resolving may enqueue nested choices; show them afterwards.
            request?.ResolvePick(optionIndex);
            TryShowNext();
        }

        private void CloseOverlay()
        {
            if (overlayRoot != null)
                Destroy(overlayRoot.gameObject);

            overlayRoot = null;
            activeRequest = null;
        }

        private bool EnsureCanvas()
        {
            if (rootCanvas != null)
                return true;

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
                rootCanvas = canvas.rootCanvas;

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
