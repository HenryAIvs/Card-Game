using System.Linq;
using UnityEngine;
using Combat.Core;
using Combat.Data.Heroes;
using Combat.Entities;
using Combat.Cards;
using Combat.Enemies;

namespace Combat.Runner
{
    public class CombatInitialiser
    {
        private readonly CombatRunner runner;

        public CombatInitialiser(CombatRunner runner)
        {
            this.runner = runner;
        }

        public void InitialiseCombat()
        {
            if (runner.IsInitialised)
            {
                Debug.LogWarning("CombatRunner: InitialiseCombat called more than once.");
                return;
            }

            if (runner.Config == null)
            {
                Debug.LogError("CombatRunner: config is not assigned.");
                return;
            }

            runner.State = new CombatState();

            CreateHeroes();
            CreateEnemies();
            BuildEnemyDeck();

            DebugLogInitialState();
            InitialiseObservedHandCounts();

            runner.IsInitialised = true;
        }

        private void CreateHeroes()
        {
            if (runner.Config.heroes == null || runner.Config.heroes.Count == 0)
            {
                Debug.LogWarning("CombatRunner: config.heroes is empty.");
                return;
            }

            for (int i = 0; i < runner.Config.heroes.Count; i++)
            {
                HeroDefinitionSO heroDef = runner.Config.heroes[i];
                if (heroDef == null)
                {
                    Debug.LogWarning($"CombatRunner: config.heroes[{i}] is null.");
                    continue;
                }

                HeroInstance hero = new HeroInstance
                {
                    definition = heroDef,
                    id = string.IsNullOrWhiteSpace(heroDef.displayName) ? heroDef.heroId : heroDef.displayName,
                    brawn = heroDef.brawn,
                    finesse = heroDef.finesse,
                    ingenuity = heroDef.ingenuity
                };

                runner.State.heroes.Add(hero);
                runner.State.lane.AddAt(runner.State.lane.entities.Count, hero);

                BuildHeroDeck(hero, heroDef);
            }
        }

        private void BuildHeroDeck(HeroInstance hero, HeroDefinitionSO heroDef)
        {
            hero.deck.drawPile.Clear();
            hero.deck.discardPile.Clear();
            hero.deck.hand.Clear();

            if (heroDef.startingDeck == null)
            {
                Debug.LogWarning($"CombatRunner: {hero.id} has no starting deck assigned.");
                return;
            }

            if (heroDef.startingDeck.entries == null)
            {
                Debug.LogWarning($"CombatRunner: {hero.id} starting deck has no entries.");
                return;
            }

            foreach (var entry in heroDef.startingDeck.entries)
            {
                if (entry == null) continue;
                if (entry.card == null) continue;
                if (entry.count <= 0) continue;

                for (int copy = 0; copy < entry.count; copy++)
                {
                    hero.deck.drawPile.Add(new CardInstance(entry.card));
                }
            }

            hero.deck.ShuffleDrawPile();
        }

        private void CreateEnemies()
        {
            if (runner.Config.enemies == null || runner.Config.enemies.Count == 0)
                return;

            foreach (var arch in runner.Config.enemies)
            {
                if (arch == null) continue;

                EnemyInstance enemy = new EnemyInstance(arch);
                runner.State.lane.AddAt(runner.State.lane.entities.Count, enemy);
            }
        }

        private void BuildEnemyDeck()
        {
            runner.State.enemyDeck.drawPile.Clear();
            runner.State.enemyDeck.discardPile.Clear();

            if (runner.Config.enemyDeck == null)
            {
                Debug.LogWarning("CombatRunner: config.enemyDeck is not assigned.");
                return;
            }

            if (runner.Config.enemyDeck.cards != null)
            {
                runner.State.enemyDeck.drawPile.AddRange(runner.Config.enemyDeck.cards);
            }

            runner.State.enemyDeck.ShuffleDrawPile();
        }

        private void InitialiseObservedHandCounts()
        {
            runner.ObservedHandCounts.Clear();

            for (int i = 0; i < runner.State.heroes.Count; i++)
            {
                HeroInstance hero = runner.State.heroes[i];
                runner.ObservedHandCounts[hero] = hero != null ? hero.deck.hand.Count : 0;
            }
        }

        private void DebugLogInitialState()
        {
            Debug.Log($"START: Round {runner.State.round} Phase {runner.State.loop.phase}");
            Debug.Log($"LANE: {string.Join(" | ", runner.State.lane.entities.Select(e => $"{e.id}({e.faction})"))}");
        }
    }
}