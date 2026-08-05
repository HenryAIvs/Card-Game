using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Combat.Entities;
using Combat.Statuses;

namespace UI.Combat
{
    public class CombatLaneEntryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private GameObject hpBarRoot;
        [SerializeField] private Image hpBarFill;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text blockText;
        [SerializeField] private Image targetHighlight;
        [SerializeField] private RectTransform targetPoint;

        [Header("Raycast")]
        [SerializeField] private Graphic raycastGraphic;

        [Header("Target Highlight")]
        [SerializeField] private Color validTargetColor = Color.yellow;
        [SerializeField] private Color latchedTargetColor = Color.red;

        private EntityInstance boundEntity;
        private bool isValidTarget = false;
        private bool isLatchedTarget = false;

        public EntityInstance BoundEntity => boundEntity;

        private void Awake()
        {
            if (hpText != null)
                hpText.gameObject.SetActive(false);

            if (hpBarRoot != null)
                hpBarRoot.SetActive(false);

            ApplyTargetHighlight();
            ApplyRaycastState();
        }

        public void Bind(EntityInstance entity)
        {
            boundEntity = entity;
            Refresh();
        }

        public void Refresh()
        {
            if (boundEntity == null)
            {
                if (nameText != null)
                    nameText.text = "Unknown";

                if (portraitImage != null)
                {
                    portraitImage.sprite = null;
                    portraitImage.enabled = false;
                }

                if (hpText != null)
                    hpText.gameObject.SetActive(false);

                if (hpBarRoot != null)
                    hpBarRoot.SetActive(false);

                if (blockText != null)
                    blockText.text = "";

                ApplyTargetHighlight();
                ApplyRaycastState();
                return;
            }

            RefreshName();
            RefreshPortrait();
            RefreshHpDisplay();
            RefreshBlockText();
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
            string entryName = boundEntity != null ? boundEntity.id : "NULL";
            Debug.Log($"LANE ENTRY POINTER ENTER | entry={entryName} | valid={isValidTarget}");

            if (!isValidTarget)
                return;

            CardTargetingSessionUI session = FindFirstObjectByType<CardTargetingSessionUI>();
            if (session != null)
                session.HandleLaneEntryPointerEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            string entryName = boundEntity != null ? boundEntity.id : "NULL";
            Debug.Log($"LANE ENTRY POINTER EXIT | entry={entryName} | valid={isValidTarget}");

            CardTargetingSessionUI session = FindFirstObjectByType<CardTargetingSessionUI>();
            if (session != null)
                session.HandleLaneEntryPointerExit(this);
        }

        private void RefreshName()
        {
            if (nameText == null)
                return;

            nameText.text = string.IsNullOrWhiteSpace(boundEntity.id) ? "Unknown" : boundEntity.id;
        }

        private void RefreshPortrait()
        {
            if (portraitImage == null)
                return;

            Sprite spriteToUse = null;

            if (boundEntity is HeroInstance hero)
            {
                if (hero.definition != null)
                    spriteToUse = hero.definition.laneSprite;
            }
            else if (boundEntity is EnemyInstance enemy)
            {
                if (enemy.archetype != null)
                    spriteToUse = enemy.archetype.laneSprite;
            }

            portraitImage.sprite = spriteToUse;
            portraitImage.enabled = spriteToUse != null;
        }

        private void RefreshHpDisplay()
        {
            if (boundEntity is not EnemyInstance enemy)
            {
                if (hpText != null)
                    hpText.gameObject.SetActive(false);

                if (hpBarRoot != null)
                    hpBarRoot.SetActive(false);

                return;
            }

            if (hpText != null)
            {
                hpText.gameObject.SetActive(true);
                hpText.text = $"{enemy.currentHp}/{enemy.maxHp} HP";
            }

            if (hpBarRoot != null)
                hpBarRoot.SetActive(true);

            if (hpBarFill != null)
            {
                float fillAmount = 0f;

                if (enemy.maxHp > 0)
                    fillAmount = (float)enemy.currentHp / enemy.maxHp;

                hpBarFill.fillAmount = Mathf.Clamp01(fillAmount);
            }
        }

        private void RefreshBlockText()
        {
            if (blockText == null)
                return;

            int blockAmount = 0;

            if (boundEntity.statuses != null && boundEntity.statuses.TryGetValue(StatusId.Block, out int value))
                blockAmount = value;

            blockText.text = $"Block: {blockAmount}";
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