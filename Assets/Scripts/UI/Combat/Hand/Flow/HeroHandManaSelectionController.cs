using System.Collections.Generic;
using UnityEngine;
using Combat.Cards;
using Combat.Core;
using Combat.Entities;
using Combat.Resources.Mana;
using Combat.Targeting;

namespace UI.Combat
{
    public class HeroHandManaSelectionController
    {
        private readonly HeroHandUI owner;

        private CardInstance pendingCard;
        private TargetResult pendingTargets;
        private readonly List<ManaColor> pendingChoices = new List<ManaColor>();
        private readonly List<ChoiceRequest> pendingRequests = new List<ChoiceRequest>();

        public bool IsAwaitingSelection => pendingCard != null && pendingRequests.Count > 0;

        // Options for the pick the player must make right now, or null.
        public ManaColor[] GetCurrentChoiceOptions()
        {
            if (!IsAwaitingSelection || pendingChoices.Count >= pendingRequests.Count)
                return null;

            return pendingRequests[pendingChoices.Count].Options;
        }

        private struct ChoiceRequest
        {
            public ManaColor[] Options;
            public string Reason;

            public ChoiceRequest(ManaColor[] options, string reason)
            {
                Options = options;
                Reason = reason;
            }
        }

        public HeroHandManaSelectionController(HeroHandUI owner)
        {
            this.owner = owner;
        }

        public bool TryStartSelection(CardInstance card, TargetResult targets, ManaCost cost)
        {
            CancelPendingSelection();

            if (owner.BoundHero == null)
                return false;

            // Simulate the payment with the real solver, recording every point
            // where the player has a genuine colour choice.
            List<ChoiceRequest> requests = new List<ChoiceRequest>();

            bool payable = ManaCostSolver.TryBuildPaymentPlan(
                owner.BoundHero.energyCurrent,
                cost,
                (feasibleOptions, reason) =>
                {
                    requests.Add(new ChoiceRequest(feasibleOptions.ToArray(), reason));
                    return feasibleOptions[0];
                },
                out _
            );

            if (!payable)
                return false;

            if (requests.Count == 0)
                return true;

            pendingCard = card;
            pendingTargets = targets;
            pendingChoices.Clear();
            pendingRequests.Clear();
            pendingRequests.AddRange(requests);

            Debug.Log(
                $"MANA SELECTION START | Card: {card.card.displayName} | Choices needed: {pendingRequests.Count}"
            );

            return true;
        }

        public void HandleEnergyClicked(HeroInstance clickedHero, ManaColor color)
        {
            if (!CanAcceptEnergyClick(clickedHero))
                return;

            ChoiceRequest currentRequest = pendingRequests[pendingChoices.Count];
            if (!IsAllowedChoice(currentRequest, color))
                return;

            pendingChoices.Add(color);

            Debug.Log(
                $"MANA PICK | {color} | {pendingChoices.Count}/{pendingRequests.Count} | Reason: {currentRequest.Reason}"
            );

            if (pendingChoices.Count >= pendingRequests.Count)
                FinalisePendingCardPlay();
        }

        public void CancelPendingSelection()
        {
            pendingCard = null;
            pendingTargets = null;
            pendingChoices.Clear();
            pendingRequests.Clear();
        }

        private bool CanAcceptEnergyClick(HeroInstance clickedHero)
        {
            if (!IsAwaitingSelection)
                return false;

            if (clickedHero != owner.BoundHero)
                return false;

            if (pendingCard == null)
                return false;

            if (pendingChoices.Count >= pendingRequests.Count)
                return false;

            return true;
        }

        private bool IsAllowedChoice(ChoiceRequest request, ManaColor color)
        {
            for (int i = 0; i < request.Options.Length; i++)
            {
                if (request.Options[i] == color)
                    return true;
            }

            return false;
        }

        private void FinalisePendingCardPlay()
        {
            if (owner.CombatRunner == null || owner.BoundHero == null || pendingCard == null)
            {
                CancelPendingSelection();
                return;
            }

            CombatState state = owner.CombatRunner.State;
            if (state == null || state.loop.phase != CombatPhase.Heroes)
            {
                CancelPendingSelection();
                return;
            }

            int choiceIndex = 0;

            bool played = HeroPlayService.TryPlayCard(
                state,
                owner.BoundHero,
                pendingCard,
                pendingTargets ?? new TargetResult(),
                (feasibleOptions, reason) =>
                {
                    if (feasibleOptions == null || feasibleOptions.Count == 0)
                        return ManaColor.Red;

                    if (feasibleOptions.Count == 1)
                        return feasibleOptions[0];

                    if (choiceIndex < pendingChoices.Count)
                    {
                        ManaColor chosen = pendingChoices[choiceIndex];
                        if (feasibleOptions.Contains(chosen))
                        {
                            choiceIndex++;
                            return chosen;
                        }
                    }

                    return feasibleOptions[0];
                },
                out PaymentPlan payment
            );

            CancelPendingSelection();

            if (played)
                owner.RefreshHand();
        }

    }
}