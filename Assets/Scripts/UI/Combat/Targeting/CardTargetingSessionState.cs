using System.Collections.Generic;
using Combat.Cards;
using Combat.Entities;
using Combat.Targeting;

namespace UI.Combat
{
    public class CardTargetingSessionState
    {
        public HeroHandUI owner;
        public HeroInstance sourceHero;
        public CardInstance card;
        public HandCardUI sourceCardUI;

        public ManualTargetRequest currentRequest;
        public TargetResult partialTargets = new TargetResult();

        public CombatLaneEntryUI hoveredEntry;
        public CombatLaneEntryUI latchedEntry;

        public CombatLaneSpaceUI hoveredSpace;
        public CombatLaneSpaceUI latchedSpace;

        public EntityInstance lockedEntity;

        public HashSet<EntityInstance> validTargets = new HashSet<EntityInstance>();
        public HashSet<int> validSpaces = new HashSet<int>();

        public HandFanLayout sourceFanLayout;

        public bool IsActive => owner != null && card != null && currentRequest != null;
        public bool IsSpaceRequest => currentRequest != null && currentRequest.IsSpaceRequest;

        public void Reset()
        {
            owner = null;
            sourceHero = null;
            card = null;
            sourceCardUI = null;
            currentRequest = null;
            partialTargets = new TargetResult();
            hoveredEntry = null;
            latchedEntry = null;
            hoveredSpace = null;
            latchedSpace = null;
            lockedEntity = null;
            validTargets.Clear();
            validSpaces.Clear();
            sourceFanLayout = null;
        }
    }
}