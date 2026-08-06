using System;
using System.Collections.Generic;
using System.Linq;
using Combat.Entities;
using Combat.Data.Effects;
using UnityEngine;

namespace Combat.Targeting
{
    public static class TargetCandidateBuilder
    {
        public static List<EntityInstance> BuildEntityCandidates(TargetContext context)
        {
            if (context == null || context.state == null || context.step == null)
                return new List<EntityInstance>();

            List<EntityInstance> lane = context.state.lane.entities;
            TargetStep step = context.step;
            EntityInstance source = context.source;

            switch (step.filter)
            {
                case TargetFilter.Self:
                    return source != null
                        ? new List<EntityInstance> { source }
                        : new List<EntityInstance>();

                case TargetFilter.Any:
                    return new List<EntityInstance>(lane);

                case TargetFilter.Neutral:
                    return lane.Where(e => e != null && e.faction == Faction.Neutral).ToList();

                case TargetFilter.Ally:
                    return lane.Where(e => IsAllyOf(source, e)).ToList();

                case TargetFilter.Adversary:
                    return lane.Where(e => IsAdversaryOf(source, e)).ToList();

                case TargetFilter.Target:
                    return new List<EntityInstance>(context.result.GetEntities(step.targetLabel));

                case TargetFilter.Space:
                    return new List<EntityInstance>();

                default:
                    return new List<EntityInstance>(lane);
            }
        }

        public static List<int> BuildSpaceCandidates(TargetContext context)
        {
            List<int> spaces = new List<int>();

            if (context == null || context.state == null || context.step == null || context.state.lane == null)
                return spaces;

            int laneCount = context.state.lane.entities.Count;
            int maxDistance = GetSpaceStepMoveDistance(context);

            if (maxDistance <= 0)
            {
                for (int i = 0; i <= laneCount; i++)
                    spaces.Add(i);

                return spaces;
            }

            List<EntityInstance> anchors = GetAnchors(context);
            if (anchors.Count == 0)
                return spaces;

            HashSet<int> validSpaces = new HashSet<int>();

            for (int i = 0; i < anchors.Count; i++)
            {
                EntityInstance anchor = anchors[i];
                if (anchor == null)
                    continue;

                int currentIndex = context.state.lane.IndexOf(anchor);
                if (currentIndex < 0)
                    continue;

                // Gap indexes run from 0..laneCount.
                // An entity at currentIndex occupies the span between:
                // - gap currentIndex
                // - gap currentIndex + 1
                //
                // Those two gaps are distance 0 and count as valid picks:
                // choosing one means "stay put", so a card that bundles a
                // move with other effects never forces the movement.
                for (int gapIndex = 0; gapIndex <= laneCount; gapIndex++)
                {
                    int distance = GetGapDistanceFromEntity(currentIndex, gapIndex);
                    if (distance <= maxDistance)
                        validSpaces.Add(gapIndex);
                }
            }

            return validSpaces.OrderBy(i => i).ToList();
        }

        // Gaps run 0..laneCount; an entity at currentIndex spans gaps
        // currentIndex and currentIndex + 1, so both count as distance 0.
        public static int GetGapDistanceFromEntity(int currentIndex, int gapIndex)
        {
            if (gapIndex < currentIndex)
                return currentIndex - gapIndex;

            if (gapIndex > currentIndex + 1)
                return gapIndex - (currentIndex + 1);

            return 0;
        }

        private static int GetSpaceStepMoveDistance(TargetContext context)
        {
            if (context == null || context.step == null || context.effects == null)
                return 0;

            return FindMoveDistanceForLabel(context.effects, context.step.label);
        }

        // Move effects can sit inside conditional wrappers (e.g. parity), so
        // the search has to recurse to find the distance for a space label.
        private static int FindMoveDistanceForLabel(List<EffectSO> effects, string label)
        {
            if (effects == null)
                return 0;

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] is MoveUpToEffectSO moveEffect && moveEffect.spaceLabel == label)
                    return Mathf.Max(0, moveEffect.maxDistance);

                if (effects[i] is ParityConditionalEffectSO conditional)
                {
                    int nested = FindMoveDistanceForLabel(conditional.effects, label);
                    if (nested > 0)
                        return nested;
                }
            }

            return 0;
        }

        public static List<EntityInstance> GetAnchors(TargetContext context)
        {
            if (context == null || context.step == null)
                return new List<EntityInstance>();

            if (context.step.anchor == TargetAnchor.Source)
            {
                return context.source != null
                    ? new List<EntityInstance> { context.source }
                    : new List<EntityInstance>();
            }

            return new List<EntityInstance>(context.result.GetEntities(context.step.anchorLabel));
        }

        public static List<EntityInstance> GetAdjacentCandidates(
            TargetContext context,
            List<EntityInstance> candidates
        )
        {
            List<EntityInstance> anchors = GetAnchors(context);
            HashSet<EntityInstance> output = new HashSet<EntityInstance>();

            for (int i = 0; i < anchors.Count; i++)
            {
                EntityInstance anchor = anchors[i];
                int index = context.state.lane.IndexOf(anchor);
                if (index < 0)
                    continue;

                if (index - 1 >= 0)
                    output.Add(context.state.lane.entities[index - 1]);

                if (index + 1 < context.state.lane.entities.Count)
                    output.Add(context.state.lane.entities[index + 1]);
            }

            return output.Where(candidates.Contains).ToList();
        }

        public static List<EntityInstance> GetNearestCandidates(
            TargetContext context,
            List<EntityInstance> candidates
        )
        {
            List<EntityInstance> anchors = GetAnchors(context);

            if (anchors.Count == 0 || candidates.Count == 0)
                return new List<EntityInstance>();

            int BestDistance(EntityInstance candidate)
            {
                int candidateIndex = context.state.lane.IndexOf(candidate);
                int best = int.MaxValue;

                for (int i = 0; i < anchors.Count; i++)
                {
                    int anchorIndex = context.state.lane.IndexOf(anchors[i]);
                    if (candidateIndex < 0 || anchorIndex < 0)
                        continue;

                    int dist = Math.Abs(candidateIndex - anchorIndex);
                    if (dist < best)
                        best = dist;
                }

                return best;
            }

            int minDist = int.MaxValue;
            for (int i = 0; i < candidates.Count; i++)
            {
                int dist = BestDistance(candidates[i]);
                if (dist < minDist)
                    minDist = dist;
            }

            return candidates.Where(c => BestDistance(c) == minDist).ToList();
        }

        public static List<EntityInstance> GetFurthestCandidates(
            TargetContext context,
            List<EntityInstance> candidates
        )
        {
            List<EntityInstance> anchors = GetAnchors(context);

            if (anchors.Count == 0 || candidates.Count == 0)
                return new List<EntityInstance>();

            int BestDistance(EntityInstance candidate)
            {
                int candidateIndex = context.state.lane.IndexOf(candidate);
                int best = int.MinValue;

                for (int i = 0; i < anchors.Count; i++)
                {
                    int anchorIndex = context.state.lane.IndexOf(anchors[i]);
                    if (candidateIndex < 0 || anchorIndex < 0)
                        continue;

                    int dist = Math.Abs(candidateIndex - anchorIndex);
                    if (dist > best)
                        best = dist;
                }

                return best;
            }

            int maxDist = int.MinValue;
            for (int i = 0; i < candidates.Count; i++)
            {
                int dist = BestDistance(candidates[i]);
                if (dist > maxDist)
                    maxDist = dist;
            }

            return candidates.Where(c => BestDistance(c) == maxDist).ToList();
        }

        private static bool IsAllyOf(EntityInstance source, EntityInstance other)
        {
            if (source == null || other == null)
                return false;

            if (source.faction == Faction.Neutral)
                return false;

            return other.faction == source.faction;
        }

        private static bool IsAdversaryOf(EntityInstance source, EntityInstance other)
        {
            if (source == null || other == null)
                return false;

            if (source.faction == Faction.Neutral || other.faction == Faction.Neutral)
                return false;

            return (source.faction == Faction.Hero && other.faction == Faction.Villain)
                || (source.faction == Faction.Villain && other.faction == Faction.Hero);
        }
    }
}