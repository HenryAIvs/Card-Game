using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Combat.Entities;

namespace UI.Combat
{
    // Shows what enemies are doing during their phase: a top-centre panel
    // announcing draws and card plays, plus a scale highlight on the acting
    // enemy's lane entry. Built entirely in code; CombatRunner's enemy turn
    // sequence spawns it on demand.
    public class EnemyTurnPresenter : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float bannerHoldTime = 0.8f;
        [SerializeField] private float cardHoldTime = 1.1f;
        [SerializeField] private float fadeTime = 0.15f;

        [Header("Acting Highlight")]
        [SerializeField] private float actingScale = 1.08f;

        private Canvas rootCanvas;
        private RectTransform panel;
        private CanvasGroup panelGroup;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI bodyText;

        private Transform actingEntry;
        private Vector3 actingEntryBaseScale = Vector3.one;

        public IEnumerator ShowBanner(string title, string body)
        {
            yield return ShowPanel(title, body, bannerHoldTime);
        }

        public IEnumerator ShowCardPlay(string title, string body)
        {
            yield return ShowPanel(title, body, cardHoldTime);
        }

        public void SetActingEnemy(EntityInstance enemy)
        {
            ClearActingEnemy();

            CombatLaneUI lane = FindFirstObjectByType<CombatLaneUI>();
            if (lane == null)
                return;

            CombatLaneEntryUI entry = lane.FindEntry(enemy);
            if (entry == null)
                return;

            actingEntry = entry.transform;
            actingEntryBaseScale = actingEntry.localScale;
            actingEntry.localScale = actingEntryBaseScale * actingScale;
        }

        public void ClearActingEnemy()
        {
            if (actingEntry != null)
                actingEntry.localScale = actingEntryBaseScale;

            actingEntry = null;
        }

        private IEnumerator ShowPanel(string title, string body, float holdTime)
        {
            if (!EnsurePanel())
                yield break;

            titleText.text = title;
            bodyText.text = body;
            bodyText.gameObject.SetActive(!string.IsNullOrEmpty(body));

            panel.gameObject.SetActive(true);

            yield return Fade(0f, 1f);
            yield return new WaitForSeconds(holdTime);
            yield return Fade(1f, 0f);

            panel.gameObject.SetActive(false);
        }

        private IEnumerator Fade(float from, float to)
        {
            if (fadeTime <= 0f)
            {
                panelGroup.alpha = to;
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                panelGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / fadeTime));
                yield return null;
            }

            panelGroup.alpha = to;
        }

        private bool EnsurePanel()
        {
            if (panel != null)
                return true;

            if (rootCanvas == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                    rootCanvas = canvas.rootCanvas;
            }

            if (rootCanvas == null)
                return false;

            panel = CreateRect("EnemyTurnPanel", rootCanvas.transform);
            panel.anchorMin = new Vector2(0.5f, 1f);
            panel.anchorMax = new Vector2(0.5f, 1f);
            panel.anchoredPosition = new Vector2(0f, -170f);
            panel.sizeDelta = new Vector2(560f, 120f);
            panel.SetAsLastSibling();

            Image background = panel.gameObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.08f, 0.1f, 0.9f);
            background.raycastTarget = false;

            panelGroup = panel.gameObject.AddComponent<CanvasGroup>();
            panelGroup.alpha = 0f;
            panelGroup.blocksRaycasts = false;
            panelGroup.interactable = false;

            titleText = CreateText("Title", panel, 30f);
            titleText.rectTransform.anchoredPosition = new Vector2(0f, 22f);
            titleText.rectTransform.sizeDelta = new Vector2(540f, 44f);

            bodyText = CreateText("Body", panel, 22f);
            bodyText.rectTransform.anchoredPosition = new Vector2(0f, -24f);
            bodyText.rectTransform.sizeDelta = new Vector2(540f, 36f);
            bodyText.color = new Color(0.85f, 0.85f, 0.85f, 1f);

            panel.gameObject.SetActive(false);
            return true;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;

            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, float fontSize)
        {
            RectTransform rect = CreateRect(name, parent);

            TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            return tmp;
        }
    }
}
