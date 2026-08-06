using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Combat.Core;
using Combat.Runner;

namespace UI.Combat
{
    // Watches for the combat outcome and shows a simple end screen with a
    // button back to the main menu. Built in code; CombatHudUI spawns it.
    public class CombatEndUI : MonoBehaviour
    {
        [SerializeField] private string menuSceneName = "MainMenu";

        private CombatRunner runner;
        private Canvas rootCanvas;
        private RectTransform overlayRoot;

        private void Update()
        {
            if (overlayRoot != null)
                return;

            if (runner == null)
                runner = FindFirstObjectByType<CombatRunner>();

            if (runner == null || runner.State == null)
                return;

            CombatOutcome outcome = runner.State.loop.outcome;
            if (outcome == CombatOutcome.None)
                return;

            BuildOverlay(outcome == CombatOutcome.HeroesWin);
        }

        private void BuildOverlay(bool heroesWon)
        {
            if (rootCanvas == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                    rootCanvas = canvas.rootCanvas;
            }

            if (rootCanvas == null)
                return;

            overlayRoot = CreateRect("CombatEndOverlay", rootCanvas.transform);
            overlayRoot.anchorMin = Vector2.zero;
            overlayRoot.anchorMax = Vector2.one;
            overlayRoot.sizeDelta = Vector2.zero;
            overlayRoot.SetAsLastSibling();

            Image dim = overlayRoot.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.8f);
            dim.raycastTarget = true;

            TextMeshProUGUI message = CreateText(
                "Message",
                overlayRoot,
                heroesWon ? "Victory!" : "You Died",
                90f
            );
            message.color = heroesWon
                ? new Color(1f, 0.85f, 0.3f, 1f)
                : new Color(0.85f, 0.2f, 0.2f, 1f);
            message.rectTransform.anchoredPosition = new Vector2(0f, 80f);
            message.rectTransform.sizeDelta = new Vector2(1200f, 140f);

            RectTransform buttonRect = CreateRect("MenuButton", overlayRoot);
            buttonRect.anchoredPosition = new Vector2(0f, -80f);
            buttonRect.sizeDelta = new Vector2(320f, 80f);

            Image background = buttonRect.gameObject.AddComponent<Image>();
            background.color = new Color(0.85f, 0.85f, 0.85f, 1f);

            Button button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(ReturnToMenu);

            TextMeshProUGUI label = CreateText("Label", buttonRect, "Return to Menu", 30f);
            label.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.sizeDelta = Vector2.zero;
            label.rectTransform.anchoredPosition = Vector2.zero;
        }

        private void ReturnToMenu()
        {
            SceneManager.LoadScene(menuSceneName);
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
