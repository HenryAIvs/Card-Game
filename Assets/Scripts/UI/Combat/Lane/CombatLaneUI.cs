using System.Collections.Generic;
using UnityEngine;
using Combat.Runner;
using Combat.Entities;

namespace UI.Combat
{
    public class CombatLaneUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatRunner combatRunner;
        [SerializeField] private Transform entriesRoot;
        [SerializeField] private CombatLaneEntryUI entryPrefab;
        [SerializeField] private CombatLaneSpaceUI spacePrefab;

        private readonly List<CombatLaneEntryUI> spawnedEntries = new List<CombatLaneEntryUI>();
        private readonly List<CombatLaneSpaceUI> spawnedSpaces = new List<CombatLaneSpaceUI>();
        private readonly List<EntityInstance> lastLaneSnapshot = new List<EntityInstance>();

        private bool hasBoundLane = false;

        private void Start()
        {
            TryBuildLane();
        }

        private void Update()
        {
            if (!hasBoundLane)
            {
                TryBuildLane();
                return;
            }

            if (LaneChanged())
            {
                RebuildLane();
                return;
            }

            RefreshEntries();
        }

        private void TryBuildLane()
        {
            if (hasBoundLane)
                return;

            if (combatRunner == null)
            {
                Debug.LogWarning("CombatLaneUI: combatRunner is not assigned.");
                return;
            }

            if (!combatRunner.IsInitialised)
                return;

            if (entriesRoot == null)
            {
                Debug.LogWarning("CombatLaneUI: entriesRoot is not assigned.");
                return;
            }

            if (entryPrefab == null)
            {
                Debug.LogWarning("CombatLaneUI: entryPrefab is not assigned.");
                return;
            }

            if (spacePrefab == null)
            {
                Debug.LogWarning("CombatLaneUI: spacePrefab is not assigned.");
                return;
            }

            RebuildLane();
            hasBoundLane = true;
        }

        private bool LaneChanged()
        {
            if (combatRunner == null || combatRunner.State == null || combatRunner.State.lane == null)
                return false;

            List<EntityInstance> currentLane = combatRunner.State.lane.entities;

            if (currentLane.Count != lastLaneSnapshot.Count)
                return true;

            for (int i = 0; i < currentLane.Count; i++)
            {
                if (currentLane[i] != lastLaneSnapshot[i])
                    return true;
            }

            return false;
        }

        private void RebuildLane()
        {
            ClearEntries();
            lastLaneSnapshot.Clear();

            if (combatRunner == null || combatRunner.State == null || combatRunner.State.lane == null)
                return;

            List<EntityInstance> currentLane = combatRunner.State.lane.entities;

            for (int spaceIndex = 0; spaceIndex <= currentLane.Count; spaceIndex++)
            {
                CombatLaneSpaceUI space = Instantiate(spacePrefab, entriesRoot);
                space.Bind(spaceIndex);
                spawnedSpaces.Add(space);

                if (spaceIndex < currentLane.Count)
                {
                    EntityInstance entity = currentLane[spaceIndex];
                    CombatLaneEntryUI entry = Instantiate(entryPrefab, entriesRoot);
                    entry.Bind(entity);
                    spawnedEntries.Add(entry);
                    lastLaneSnapshot.Add(entity);
                }
            }
        }

        private void RefreshEntries()
        {
            for (int i = 0; i < spawnedEntries.Count; i++)
            {
                CombatLaneEntryUI entry = spawnedEntries[i];
                if (entry != null)
                    entry.Refresh();
            }
        }

        private void ClearEntries()
        {
            for (int i = spawnedEntries.Count - 1; i >= 0; i--)
            {
                if (spawnedEntries[i] != null)
                    Destroy(spawnedEntries[i].gameObject);
            }

            for (int i = spawnedSpaces.Count - 1; i >= 0; i--)
            {
                if (spawnedSpaces[i] != null)
                    Destroy(spawnedSpaces[i].gameObject);
            }

            spawnedEntries.Clear();
            spawnedSpaces.Clear();

            if (entriesRoot == null)
                return;

            for (int i = entriesRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(entriesRoot.GetChild(i).gameObject);
            }
        }
    }
}