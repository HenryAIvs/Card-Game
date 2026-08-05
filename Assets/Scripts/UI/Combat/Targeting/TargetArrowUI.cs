using UnityEngine;

namespace UI.Combat
{
    public class TargetArrowUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private RectTransform lineRect;

        [Header("Style")]
        [SerializeField] private float lineThickness = 18f;

        private bool isShown = false;

        private void Awake()
        {
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            if (canvasRect == null && canvas != null)
                canvasRect = canvas.transform as RectTransform;

            Hide();
        }

        public void Show()
        {
            isShown = true;

            if (lineRect != null)
                lineRect.gameObject.SetActive(true);
        }

        public void Hide()
        {
            isShown = false;

            if (lineRect != null)
                lineRect.gameObject.SetActive(false);
        }

        public void SetEndpointsScreenSpace(Vector2 startScreen, Vector2 endScreen)
        {
            if (!isShown || canvasRect == null || lineRect == null)
                return;

            Camera eventCamera = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                eventCamera = canvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                startScreen,
                eventCamera,
                out Vector2 startLocal
            );

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                endScreen,
                eventCamera,
                out Vector2 endLocal
            );

            Vector2 delta = endLocal - startLocal;
            float length = delta.magnitude;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            lineRect.pivot = new Vector2(0f, 0.5f);
            lineRect.anchoredPosition = startLocal;
            lineRect.localRotation = Quaternion.Euler(0f, 0f, angle);
            lineRect.sizeDelta = new Vector2(length, lineThickness);
        }
    }
}