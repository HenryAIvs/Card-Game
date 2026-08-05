using System.Collections.Generic;
using UnityEngine;
using Combat.Cards;
using Combat.Core;
using Combat.Resources.Mana;
using Combat.Targeting;

namespace UI.Combat
{
    public class HeroHandPlayController
    {
        private readonly HeroHandUI owner;
        private readonly HeroHandManaSelectionController manaSelectionController;

        public HeroHandPlayController(
            HeroHandUI owner,
            HeroHandManaSelectionController manaSelectionController
        )
        {
            this.owner = owner;
            this.manaSelectionController = manaSelectionController;
        }

        public bool CanCurrentlyPlayCard(CardInstance card)
        {
            if (!HasValidPlayContext(card, requireHeroesPhase: true))
                return false;

            ManaCost runtimeCost = card.card.cost != null ? card.card.cost.ToRuntime() : null;
            EnergyPool simulatedPool = owner.BoundHero.energyCurrent;

            return ManaCostSolver.TryBuildPaymentPlan(
                simulatedPool,
                runtimeCost,
                ChooseManaColorForPreview,
                out PaymentPlan payment
            );
        }

        public bool CanBeginDragPlay(CardInstance card)
        {
            return CanCurrentlyPlayCard(card);
        }

        public bool TryPlayCardFromDrag(CardInstance card)
        {
            if (card == null)
                return false;

            if (CardRequiresManualTargeting(card))
            {
                Debug.Log($"DRAG TARGETING SHOULD HAVE STARTED SESSION | {card.card.displayName}");
                return false;
            }

            return TryPlayCard(card, new TargetResult());
        }

        public bool TryPlayCard(CardInstance card, TargetResult chosenTargets)
        {
            Debug.Log(
                $"TRY PLAY CARD START | card={(card != null && card.card != null ? card.card.displayName : "NULL")} | " +
                $"boundHero={(owner.BoundHero != null ? owner.BoundHero.id : "NULL")} | " +
                $"phase={(owner.CombatRunner != null && owner.CombatRunner.State != null ? owner.CombatRunner.State.loop.phase.ToString() : "NULL")} | " +
                $"interactionLocked={owner.IsInteractionLocked}"
            );

            if (!HasValidPlayContext(card, requireHeroesPhase: true))
            {
                Debug.Log("TRY PLAY CARD FAIL | HasValidPlayContext returned false");
                return false;
            }

            CombatState state = owner.CombatRunner.State;
            ManaCost runtimeCost = card.card.cost != null ? card.card.cost.ToRuntime() : null;

            bool selectionStarted = manaSelectionController.TryStartSelection(card, chosenTargets, runtimeCost);
            Debug.Log($"TRY PLAY CARD | TryStartSelection returned {selectionStarted}");

            if (!selectionStarted)
                return false;

            Debug.Log($"TRY PLAY CARD | IsAwaitingSelection={manaSelectionController.IsAwaitingSelection}");

            if (manaSelectionController.IsAwaitingSelection)
            {
                Debug.Log("TRY PLAY CARD PAUSE | awaiting mana selection");
                return true;
            }

            bool played = HeroPlayService.TryPlayCard(
                state,
                owner.BoundHero,
                card,
                chosenTargets,
                ChooseManaColorForPreview,
                out PaymentPlan payment
            );

            Debug.Log(
                $"TRY PLAY CARD RESULT | played={played} | spendR={payment.spendRed} | spendY={payment.spendYellow} | spendB={payment.spendBlue}"
            );

            if (played)
                owner.RefreshHand();

            return played;
        }

        public bool CardRequiresManualTargeting(CardInstance card)
        {
            if (card == null || card.card == null)
                return false;

            if (owner.CombatRunner == null || owner.CombatRunner.State == null || owner.BoundHero == null)
                return false;

            return TargetingDecisionService.RequiresManualTargeting(
                owner.CombatRunner.State,
                owner.BoundHero,
                card.card.targeting,
                card.card.effects
            );
        }

        private bool HasValidPlayContext(CardInstance card, bool requireHeroesPhase)
        {
            if (owner.CombatRunner == null)
            {
                Debug.Log("PLAY CONTEXT FAIL | CombatRunner is null");
                return false;
            }

            if (owner.BoundHero == null)
            {
                Debug.Log("PLAY CONTEXT FAIL | BoundHero is null");
                return false;
            }

            if (card == null)
            {
                Debug.Log("PLAY CONTEXT FAIL | card is null");
                return false;
            }

            if (card.card == null)
            {
                Debug.Log("PLAY CONTEXT FAIL | card.card is null");
                return false;
            }

            CombatState state = owner.CombatRunner.State;
            if (state == null)
            {
                Debug.Log("PLAY CONTEXT FAIL | state is null");
                return false;
            }

            if (requireHeroesPhase && state.loop.phase != CombatPhase.Heroes)
            {
                Debug.Log($"PLAY CONTEXT FAIL | wrong phase {state.loop.phase}");
                return false;
            }

            if (owner.IsInteractionLocked)
            {
                Debug.Log("PLAY CONTEXT FAIL | owner.IsInteractionLocked is true");
                return false;
            }

            return true;
        }

        private ManaColor ChooseManaColorForPreview(
            List<ManaColor> feasibleOptions,
            string reason
        )
        {
            if (feasibleOptions == null || feasibleOptions.Count == 0)
                return ManaColor.Red;

            return feasibleOptions[0];
        }
    }
}