using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Menu
{
    // Minimal start menu: title plus a Play button that loads the combat
    // scene. The whole UI is built in code so the scene only needs this one
    // component.
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private string combatSceneName = "CombatScene";
        [SerializeField] private string gameTitle = "Card Game";

        private void Start()
        {
            BuildMenu();
        }

        private void BuildMenu()
        {
            GameObject canvasGo = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.layer = 5;

            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            TextMeshProUGUI title = CreateText("Title", canvas.transform, gameTitle, 80f);
            title.rectTransform.anchoredPosition = new Vector2(0f, 160f);
            title.rectTransform.sizeDelta = new Vector2(1200f, 120f);

            RectTransform buttonRect = CreateRect("PlayButton", canvas.transform);
            buttonRect.anchoredPosition = new Vector2(0f, -60f);
            buttonRect.sizeDelta = new Vector2(300f, 90f);

            Image background = buttonRect.gameObject.AddComponent<Image>();
            background.color = new Color(0.85f, 0.85f, 0.85f, 1f);

            Button button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(Play);

            TextMeshProUGUI label = CreateText("Label", buttonRect, "Play", 40f);
            label.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.sizeDelta = Vector2.zero;
            label.rectTransform.anchoredPosition = Vector2.zero;
        }

        private void Play()
        {
            SceneManager.LoadScene(combatSceneName);
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
