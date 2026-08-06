using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.Combat
{
    public class CombatLaneSpaceUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] private Image targetHighlight;
        [SerializeField] private RectTransform targetPoint;

        [Header("Raycast")]
        [SerializeField] private Graphic raycastGraphic;

        [Header("Target Highlight")]
        [SerializeField] private Color validTargetColor = Color.yellow;
        [SerializeField] private Color latchedTargetColor = Color.red;

        [Header("Flash")]
        [SerializeField] private float flashSpeed = 6f;
        [SerializeField, Range(0f, 1f)] private float flashMinAlpha = 0.15f;
        [SerializeField, Range(0f, 1f)] private float flashMaxAlpha = 0.6f;

        private int spaceIndex = -1;
        private bool isValidTarget = false;
        private bool isLatchedTarget = false;

        public int SpaceIndex => spaceIndex;

        private void Awake()
        {
            ApplyTargetHighlight();
            ApplyRaycastState();
        }

        private void Update()
        {
            // Valid spaces pulse yellow; a latched space stays solid.
            if (!isValidTarget || isLatchedTarget || targetHighlight == null)
                return;

            float t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * flashSpeed);

            Color color = validTargetColor;
            color.a = Mathf.Lerp(flashMinAlpha, flashMaxAlpha, t);
            targetHighlight.color = color;
        }

        public void Bind(int index)
        {
            spaceIndex = index;
            ApplyTargetHighlight();
            ApplyRaycastState();
        }

        public void SetTargetingState(bool valid, bool latched)
        {
            isValidTarget = valid;
            isLatchedTarget = latched && valid;

            ApplyTargetHighlight();
            ApplyRaycastState();
        }

        public Vector2 GetTargetPointScreenSpace()
        {
            RectTransform point = targetPoint != null ? targetPoint : transform as RectTransform;
            return RectTransformUtility.WorldToScreenPoint(null, point.position);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isValidTarget)
                return;

            CardTargetingSessionUI session = FindFirstObjectByType<CardTargetingSessionUI>();
            if (session != null)
                session.HandleLaneSpacePointerEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CardTargetingSessionUI session = FindFirstObjectByType<CardTargetingSessionUI>();
            if (session != null)
                session.HandleLaneSpacePointerExit(this);
        }

        private void ApplyTargetHighlight()
        {
            if (targetHighlight == null)
                return;

            bool active = isValidTarget || isLatchedTarget;
            targetHighlight.gameObject.SetActive(active);

            if (!active)
                return;

            targetHighlight.color = isLatchedTarget ? latchedTargetColor : validTargetColor;
        }

        private void ApplyRaycastState()
        {
            if (raycastGraphic != null)
                raycastGraphic.raycastTarget = isValidTarget;
        }
    }
}