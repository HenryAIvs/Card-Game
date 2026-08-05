using System;
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

            if (!TryBuildChoiceRequests(cost, out List<ChoiceRequest> requests))
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

        private bool TryBuildChoiceRequests(ManaCost cost, out List<ChoiceRequest> requests)
        {
            requests = new List<ChoiceRequest>();

            if (owner.BoundHero == null)
                return false;

            if (cost == null || cost.costs == null)
                return true;

            Dictionary<ManaColor, int> available = BuildAvailableManaMap(owner.BoundHero.energyCurrent);

            for (int i = 0; i < cost.costs.Count; i++)
            {
                ChoiceCost chunk = cost.costs[i];

                if (chunk.options == null || chunk.options.Length == 0 || chunk.amount <= 0)
                    continue;

                string reason = GetChoiceReason(chunk.options.Length);

                for (int pip = 0; pip < chunk.amount; pip++)
                {
                    List<ManaColor> feasible = BuildFeasibleOptions(chunk, available);

                    if (feasible.Count == 0)
                        return false;

                    if (feasible.Count == 1)
                    {
                        available[feasible[0]]--;
                    }
                    else
                    {
                        requests.Add(new ChoiceRequest(feasible.ToArray(), reason));
                        available[feasible[0]]--;
                    }
                }
            }

            return true;
        }

        private List<ManaColor> BuildFeasibleOptions(
            ChoiceCost chunk,
            Dictionary<ManaColor, int> available
        )
        {
            List<ManaColor> feasible = new List<ManaColor>();

            for (int i = 0; i < chunk.options.Length; i++)
            {
                ManaColor option = chunk.options[i];

                if (feasible.Contains(option))
                    continue;

                if (available.TryGetValue(option, out int amountAvailable) && amountAvailable > 0)
                    feasible.Add(option);
            }

            return feasible;
        }

        private string GetChoiceReason(int optionCount)
        {
            if (optionCount == 1)
                return "forced";

            if (optionCount == 2)
                return "flex";

            return "generic";
        }

        private Dictionary<ManaColor, int> BuildAvailableManaMap(EnergyPool pool)
        {
            Dictionary<ManaColor, int> available = new Dictionary<ManaColor, int>();

            Array allColors = Enum.GetValues(typeof(ManaColor));
            for (int i = 0; i < allColors.Length; i++)
            {
                ManaColor color = (ManaColor)allColors.GetValue(i);
                available[color] = pool.Get(color);
            }

            return available;
        }
    }
}