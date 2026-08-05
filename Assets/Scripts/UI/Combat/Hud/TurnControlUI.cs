using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Combat.Runner;
using Combat.Core;

namespace UI.Combat
{
    public class TurnControlUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatRunner runner;
        [SerializeField] private TMP_Text phaseText;
        [SerializeField] private Button endTurnButton;
        [SerializeField] private TMP_Text endTurnButtonText;

        private CombatState state;
        private bool hasBoundState = false;

        private void Start()
        {
            if (runner == null)
                runner = FindObjectOfType<CombatRunner>();

            if (endTurnButton != null)
            {
                endTurnButton.onClick.RemoveAllListeners();
                endTurnButton.onClick.AddListener(OnAdvanceButtonClicked);
            }

            TryBindState();
            Refresh();
        }

        private void Update()
        {
            if (!hasBoundState)
            {
                TryBindState();
                return;
            }

            Refresh();
        }

        private void TryBindState()
        {
            if (hasBoundState)
                return;

            if (runner == null)
                return;

            if (!runner.IsInitialised)
                return;

            state = runner.State;

            if (state == null)
                return;

            hasBoundState = true;
        }

        private void OnAdvanceButtonClicked()
        {
            if (state == null)
                return;

            if (state.loop.phase == CombatPhase.EndCombat)
                return;

            state.loop.Advance(state);
            Refresh();
        }

        private void Refresh()
        {
            if (phaseText == null)
                return;

            if (state == null)
            {
                phaseText.text = "Combat not ready";

                if (endTurnButton != null)
                    endTurnButton.interactable = false;

                return;
            }

            phaseText.text = $"Round {state.round} - {state.loop.phase}";

            if (endTurnButtonText != null)
            {
                endTurnButtonText.text = GetButtonLabel(state.loop.phase);
            }

            if (endTurnButton != null)
            {
                endTurnButton.interactable = state.loop.phase != CombatPhase.EndCombat;
            }
        }

        private string GetButtonLabel(CombatPhase phase)
        {
            if (phase == CombatPhase.Heroes)
                return "End Turn";

            if (phase == CombatPhase.EndCombat)
                return "Combat Over";

            return "Next";
        }
    }
}