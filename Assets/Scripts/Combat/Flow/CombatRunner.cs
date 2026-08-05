using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Combat.Core;
using Combat.Data.Combat;
using Combat.Entities;
using Combat.Cards;
using UI.Combat;

namespace Combat.Runner
{
    internal struct DrawRequest
    {
        public HeroInstance hero;
        public CardInstance card;

        public DrawRequest(HeroInstance hero, CardInstance card)
        {
            this.hero = hero;
            this.card = card;
        }
    }

    public class CombatRunner : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private CombatConfigSO config;

        private CombatState state;
        private bool isInitialised = false;
        private bool isStartRoundSequenceRunning = false;
        private bool isDrawQueueRunning = false;
        private bool isCardFlowLocked = false;

        private readonly Dictionary<HeroInstance, int> observedHandCounts = new();
        private readonly Queue<DrawRequest> drawQueue = new();

        private HeroHandUI cachedHandUI;

        private CombatInitialiser initialiser;
        private CombatRoundSequenceController roundSequenceController;
        private CombatDrawQueueController drawQueueController;

        public CombatState State => state;
        public bool IsInitialised => isInitialised;
        public bool IsCardFlowLocked => isCardFlowLocked;

        internal CombatConfigSO Config => config;

        internal bool IsStartRoundSequenceRunning
        {
            get => isStartRoundSequenceRunning;
            set => isStartRoundSequenceRunning = value;
        }

        internal bool IsDrawQueueRunning
        {
            get => isDrawQueueRunning;
            set => isDrawQueueRunning = value;
        }

        internal bool IsInitialisedInternal
        {
            get => isInitialised;
            set => isInitialised = value;
        }

        internal bool IsCardFlowLockedInternal
        {
            get => isCardFlowLocked;
            set => isCardFlowLocked = value;
        }

        internal CombatState StateInternal
        {
            get => state;
            set => state = value;
        }

        internal Dictionary<HeroInstance, int> ObservedHandCounts => observedHandCounts;
        internal Queue<DrawRequest> DrawQueue => drawQueue;

        internal HeroHandUI CachedHandUI
        {
            get => cachedHandUI;
            set => cachedHandUI = value;
        }

        private void Awake()
        {
            initialiser = new CombatInitialiser(this);
            drawQueueController = new CombatDrawQueueController(this);
            roundSequenceController = new CombatRoundSequenceController(this, drawQueueController);
        }

        private void Start()
        {
            InitialiseCombat();
        }

        private void Update()
        {
            if (!CanTick())
                return;

            drawQueueController.EnsureHandUI();

            if (roundSequenceController.ShouldStartRoundSequence())
            {
                StartCoroutine(roundSequenceController.RunStartRoundSequence());
                return;
            }

            drawQueueController.DetectMidTurnDraws();
        }

        public void InitialiseCombat()
        {
            initialiser.InitialiseCombat();
        }

        private bool CanTick()
        {
            return isInitialised && state != null && !state.IsCombatOver;
        }

        public HeroInstance GetHero(int index)
        {
            if (state == null) return null;
            if (index < 0 || index >= state.heroes.Count) return null;
            return state.heroes[index];
        }

        public int GetHeroCount()
        {
            if (state == null) return 0;
            return state.heroes.Count;
        }
    }
}