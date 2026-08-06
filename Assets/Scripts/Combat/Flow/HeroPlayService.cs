using System.Collections.Generic;
using Combat.Conditions;
using Combat.Core;
using Combat.Entities;
using Combat.Resolution;
using Combat.Resources.Mana;
using Combat.Data.Cards;
using Combat.Data.Effects;
using Combat.Resolution.Types;
using Combat.Targeting;
using UnityEngine;

namespace Combat.Cards
{
    public static class HeroPlayService
    {
        public static bool TryPlayCard(
            CombatState state,
            HeroInstance hero,
            CardInstance cardInstance,
            TargetResult chosenTargets,
            ManaCostSolver.ChooseManaColorFn chooser,
            out PaymentPlan payment
        )
        {
            payment = new PaymentPlan();

            if (state == null || hero == null || cardInstance == null)
                return false;

            if (chosenTargets == null)
                chosenTargets = new TargetResult();

            if (state.conditions.Has(hero, ConditionIds.Unconscious))
                return false;

            if (!hero.deck.hand.Contains(cardInstance))
                return false;

            if (cardInstance.card == null)
                return false;

            var runtimeCost = cardInstance.card.cost != null
                ? cardInstance.card.cost.ToRuntime()
                : null;

            if (!ManaCostSolver.TryBuildPaymentPlan(hero.energyCurrent, runtimeCost, chooser, out payment))
                return false;

            hero.energyCurrent.red -= payment.spendRed;
            hero.energyCurrent.yellow -= payment.spendYellow;
            hero.energyCurrent.blue -= payment.spendBlue;

            Debug.Log(
                $"PLAY CARD | {hero.id} played {cardInstance.card.displayName} | " +
                $"Hand size at play time: {hero.deck.hand.Count}"
            );

            if (CardTagUtil.HasTag(cardInstance.card, CardTag.Slow))
            {
                List<EffectSO> resolvedSlowEffects = ResolveEffectsForSlowPlay(
                    hero,
                    cardInstance.card.effects
                );

                Debug.Log(
                    $"SLOW CARD | {cardInstance.card.displayName} queued with {resolvedSlowEffects.Count} resolved effects."
                );

                state.slowQueue.Add(new SlowCardPlay(
                    hero,
                    cardInstance.card,
                    cardInstance.card.targeting,
                    resolvedSlowEffects
                ));

                hero.deck.DiscardFromHand(cardInstance);
                return true;
            }

            ResolveMissingTargetSteps(
                state,
                hero,
                cardInstance.card.targeting,
                cardInstance.card.effects,
                chosenTargets
            );

            EffectExecutor.ExecuteAbility(
                state,
                hero,
                chosenTargets,
                cardInstance.card.effects
            );

            hero.deck.DiscardFromHand(cardInstance);
            return true;
        }

        // Manual targeting only fills the labels the player picked; steps that
        // auto-resolve (Nearest, All, ...) are still empty here and must be
        // resolved against the current board before the effects run.
        private static void ResolveMissingTargetSteps(
            CombatState state,
            HeroInstance hero,
            List<TargetStep> steps,
            List<EffectSO> effects,
            TargetResult targets
        )
        {
            if (steps == null || targets == null)
                return;

            List<EntityInstance> ChooseEntities(List<EntityInstance> options, int count)
            {
                var picked = new List<EntityInstance>();
                for (int i = 0; i < Mathf.Min(count, options.Count); i++)
                    picked.Add(options[i]);
                return picked;
            }

            List<int> ChooseSpaces(List<int> options, int count)
            {
                var picked = new List<int>();
                for (int i = 0; i < Mathf.Min(count, options.Count); i++)
                    picked.Add(options[i]);
                return picked;
            }

            for (int i = 0; i < steps.Count; i++)
            {
                TargetStep step = steps[i];
                if (step == null || string.IsNullOrEmpty(step.label))
                    continue;

                bool alreadyChosen = step.filter == TargetFilter.Space
                    ? targets.spaces.ContainsKey(step.label)
                    : targets.selections.ContainsKey(step.label);

                if (alreadyChosen)
                    continue;

                TargetResolver.ResolveStep(state, hero, step, targets, effects, ChooseEntities, ChooseSpaces);

                Debug.Log(
                    $"AUTO TARGET RESOLVE | {step.specifier} {step.filter} -> {step.label} | " +
                    $"Resolved: {targets.GetEntities(step.label).Count} entities, {targets.GetSpaces(step.label).Count} spaces"
                );
            }
        }

        public static bool TryPlayCard(
            CombatState state,
            HeroInstance hero,
            CardInstance cardInstance,
            ManaCostSolver.ChooseManaColorFn chooser,
            out PaymentPlan payment
        )
        {
            return TryPlayCard(
                state,
                hero,
                cardInstance,
                new TargetResult(),
                chooser,
                out payment
            );
        }

        private static List<EffectSO> ResolveEffectsForSlowPlay(
            HeroInstance hero,
            List<EffectSO> sourceEffects
        )
        {
            List<EffectSO> resolved = new List<EffectSO>();

            if (sourceEffects == null)
                return resolved;

            for (int i = 0; i < sourceEffects.Count; i++)
            {
                AddResolvedEffect(hero, sourceEffects[i], resolved);
            }

            return resolved;
        }

        private static void AddResolvedEffect(
            HeroInstance hero,
            EffectSO effect,
            List<EffectSO> output
        )
        {
            if (effect == null)
                return;

            if (effect is ParityConditionalEffectSO parityEffect)
            {
                int handSize = hero.deck.hand.Count;
                bool isOdd = (handSize % 2) == 1;

                bool conditionMet =
                    (parityEffect.parity == ParityType.Left && isOdd) ||
                    (parityEffect.parity == ParityType.Right && !isOdd);

                Debug.Log(
                    $"SLOW PARITY RESOLVE | Hero: {hero.id} | Hand size: {handSize} | " +
                    $"Parity: {parityEffect.parity} | Odd: {isOdd} | Condition met: {conditionMet}"
                );

                if (!conditionMet)
                    return;

                if (parityEffect.effects == null)
                    return;

                for (int i = 0; i < parityEffect.effects.Count; i++)
                {
                    AddResolvedEffect(hero, parityEffect.effects[i], output);
                }

                return;
            }

            Debug.Log($"SLOW EFFECT ADDED | {effect.name}");
            output.Add(effect);
        }
    }
}