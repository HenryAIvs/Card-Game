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

        private readonly Dictionary<HeroInstance, int> observedHandCounts = new();
        private readonly Queue<DrawRequest> drawQueue = new();

        private CombatInitialiser initialiser;
        private CombatRoundSequenceController roundSequenceController;
        private CombatDrawQueueController drawQueueController;

        public CombatState State { get; internal set; }
        public bool IsInitialised { get; internal set; }
        public bool IsCardFlowLocked { get; internal set; }

        internal CombatConfigSO Config => config;
        internal bool IsStartRoundSequenceRunning { get; set; }
        internal bool IsDrawQueueRunning { get; set; }
        internal HeroHandUI CachedHandUI { get; set; }
        internal Dictionary<HeroInstance, int> ObservedHandCounts => observedHandCounts;
        internal Queue<DrawRequest> DrawQueue => drawQueue;

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
            return IsInitialised && State != null && !State.IsCombatOver;
        }

        public HeroInstance GetHero(int index)
        {
            if (State == null) return null;
            if (index < 0 || index >= State.heroes.Count) return null;
            return State.heroes[index];
        }

        public int GetHeroCount()
        {
            return State != null ? State.heroes.Count : 0;
        }
    }
}
