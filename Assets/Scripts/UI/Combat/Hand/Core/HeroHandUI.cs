using System.Collections;
using UnityEngine;
using Combat.Runner;
using Combat.Entities;
using Combat.Cards;
using Combat.Targeting;
using Combat.Resources.Mana;

namespace UI.Combat
{
    public class HeroHandUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatRunner combatRunner;
        [SerializeField] private Transform cardContainer;
        [SerializeField] private HandCardUI cardPrefab;
        [SerializeField] private CardTargetingSessionUI targetingSession;

        [Header("Draw Animation")]
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private float drawAnimDuration = 0.28f;
        [SerializeField] private float drawStartOffsetX = 260f;
        [SerializeField] private float drawStartOffsetY = 40f;
        [SerializeField] private float drawStartScale = 1.08f;
        [SerializeField] private float drawStartRotationZ = -12f;

        [Header("Display")]
        [SerializeField] private int selectedHeroIndex = 0;

        private HeroInstance boundHero;
        private int lastKnownHandCount = -1;
        private int visualRefreshLockCount = 0;

        private HeroHandRenderer handRenderer;
        private HeroHandCardLookup cardLookup;
        private HeroHandDrawAnimator drawAnimator;
        private HeroHandPlayController playController;
        private HeroHandManaSelectionController manaSelectionController;
        private HeroHandBindingController bindingController;
        private HeroHandInteractionState interactionState;

        public CombatRunner CombatRunner => combatRunner;
        public HeroInstance BoundHero => boundHero;
        public int SelectedHeroIndex => selectedHeroIndex;
        public CardTargetingSessionUI TargetingSession => targetingSession;

        public bool IsAwaitingManaSelection => interactionState.IsAwaitingManaSelection();
        public bool IsBusyWithCardFlow => interactionState.IsBusyWithCardFlow();
        public bool IsInteractionLocked => interactionState.IsInteractionLocked();
        public bool IsVisualRefreshLocked => visualRefreshLockCount > 0;

        public float DrawAnimDuration => drawAnimDuration;
        public float DrawStartOffsetX => drawStartOffsetX;
        public float DrawStartOffsetY => drawStartOffsetY;
        public float DrawStartScale => drawStartScale;
        public float DrawStartRotationZ => drawStartRotationZ;

        private void Awake()
        {
            cardLookup = new HeroHandCardLookup(this);
            handRenderer = new HeroHandRenderer(this, cardLookup);
            drawAnimator = new HeroHandDrawAnimator(this);
            manaSelectionController = new HeroHandManaSelectionController(this);
            playController = new HeroHandPlayController(this, manaSelectionController);
            bindingController = new HeroHandBindingController(this);
            interactionState = new HeroHandInteractionState(this);

            EnsureRootCanvas();
        }

        private void OnEnable()
        {
            SelectedHeroEnergyUI.OnEnergyClicked += HandleEnergyClicked;
        }

        private void OnDisable()
        {
            SelectedHeroEnergyUI.OnEnergyClicked -= HandleEnergyClicked;
        }

        private void Start()
        {
            bindingController.InitialiseBindingAtStart();
        }

        private void Update()
        {
            bindingController.UpdateBinding();

            if (boundHero == null)
                return;

            if (!interactionState.IsDrawFlowAnimating() && !IsVisualRefreshLocked)
                RefreshHandIfNeeded(force: false);

            RefreshPlayableStates();
        }

        private void HandleEnergyClicked(HeroInstance clickedHero, ManaColor color)
        {
            manaSelectionController.HandleEnergyClicked(clickedHero, color);
        }

        public void SetSelectedHeroIndex(int newIndex)
        {
            SetSelectedHeroIndex(newIndex, forceRefresh: true);
        }

        public void SetSelectedHeroIndex(int newIndex, bool forceRefresh)
        {
            if (newIndex < 0)
                newIndex = 0;

            selectedHeroIndex = newIndex;
            bindingController.RebindSelectedHero(forceRefresh);
        }

        public void RefreshHand(bool immediate = true)
        {
            Debug.Log(
                $"HAND UI REFRESH | immediate={immediate} | boundHero={(boundHero != null ? boundHero.id : "NULL")} | handCount={(boundHero != null ? boundHero.deck.hand.Count : -1)} | visualLock={IsVisualRefreshLocked}"
            );

            handRenderer.RefreshHand(immediate);
            lastKnownHandCount = boundHero != null ? boundHero.deck.hand.Count : -1;
        }

        public void RefreshPlayableStates()
        {
            handRenderer.RefreshPlayableStates();
        }

        public HandCardUI FindCardUI(CardInstance targetCard)
        {
            return cardLookup.FindCardUI(targetCard);
        }

        public IEnumerator PlayDrawAnimation(CardInstance drawnCard)
        {
            Debug.Log(
                $"HAND UI PLAY DRAW ANIMATION | boundHero={(boundHero != null ? boundHero.id : "NULL")} | card={(drawnCard != null ? drawnCard.card.displayName : "NULL")}"
            );

            yield return drawAnimator.PlayDrawAnimation(drawnCard);
        }

        public bool CanCurrentlyPlayCard(CardInstance card)
        {
            return playController.CanCurrentlyPlayCard(card);
        }

        public bool CanBeginDragPlay(CardInstance card)
        {
            return playController.CanBeginDragPlay(card);
        }

        public bool CardRequiresManualTargeting(CardInstance card)
        {
            return playController.CardRequiresManualTargeting(card);
        }

        public bool TryBeginTargeting(CardInstance card, HandCardUI sourceCardUI)
        {
            if (card == null || card.card == null || sourceCardUI == null)
                return false;

            if (combatRunner == null || combatRunner.State == null || boundHero == null)
                return false;

            EnsureTargetingSession();

            if (targetingSession == null)
            {
                Debug.LogWarning("HeroHandUI: no CardTargetingSessionUI found in scene.");
                return false;
            }

            if (!TargetingDecisionService.TryGetFirstManualTargetRequest(
                combatRunner.State,
                boundHero,
                card.card.targeting,
                card.card.effects,
                out ManualTargetRequest request
            ))
            {
                return false;
            }

            return targetingSession.Begin(this, boundHero, card, sourceCardUI, request);
        }

        public bool TryPlayCardFromDrag(CardInstance card)
        {
            return playController.TryPlayCardFromDrag(card);
        }

        public bool TryPlayCard(CardInstance card, TargetResult chosenTargets)
        {
            return playController.TryPlayCard(card, chosenTargets);
        }

        public Transform GetCardContainer()
        {
            return cardContainer;
        }

        public HandCardUI GetCardPrefab()
        {
            return cardPrefab;
        }

        public HeroHandCardLookup GetCardLookup()
        {
            return cardLookup;
        }

        public HeroHandManaSelectionController GetManaSelectionController()
        {
            return manaSelectionController;
        }

        public Canvas GetRootCanvas()
        {
            EnsureRootCanvas();
            return rootCanvas;
        }

        public void EnsureCombatRunner()
        {
            if (combatRunner == null)
                combatRunner = FindFirstObjectByType<CombatRunner>();
        }

        public void EnsureTargetingSession()
        {
            if (targetingSession == null)
                targetingSession = FindFirstObjectByType<CardTargetingSessionUI>();
        }

        public void EnsureRootCanvas()
        {
            if (rootCanvas == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                    rootCanvas = canvas.rootCanvas;
            }
        }

        public void BeginVisualRefreshLock()
        {
            visualRefreshLockCount++;
        }

        public void EndVisualRefreshLock()
        {
            visualRefreshLockCount = Mathf.Max(0, visualRefreshLockCount - 1);
        }

        public void SetBoundHero(HeroInstance hero)
        {
            Debug.Log($"HAND UI SET BOUND HERO | {(hero != null ? hero.id : "NULL")}");
            boundHero = hero;
        }

        public void ResetKnownHandCount()
        {
            lastKnownHandCount = -1;
        }

        public void RefreshHandIfNeeded(bool force)
        {
            if (cardContainer == null || cardPrefab == null || boundHero == null)
                return;

            if (!force && IsVisualRefreshLocked)
                return;

            int currentHandCount = boundHero.deck.hand.Count;

            if (!force && currentHandCount == lastKnownHandCount)
                return;

            RefreshHand();
        }
    }
}