using UnityEngine;
using System.Collections.Generic;
using OrbitGuard.Core;

namespace OrbitGuard.Managers
{
    public class DebrisClusterManager : MonoBehaviour
    {
        public static DebrisClusterManager Instance { get; private set; }

        public List<GameObject> fragmentPrefabs; 
        public Transform spawnParent;
        public int fragmentCount = 20;
        public ParticleSystem untrackedHaze;

        [Header("Cluster Spread (local Unity meters)")]
        public float baseSemiMajorAxis = 2.2f;
        public float axisRandomRange = 0.6f;
        public float inclinationRandomRangeDegrees = 25f;
        public float periodBaseSeconds = 28f;
        public float periodRandomRange = 8f;

        private List<GameObject> activeFragments = new List<GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void GenerateCluster()
        {
            foreach (var frag in activeFragments) Destroy(frag);
            activeFragments.Clear();

            if (fragmentPrefabs == null || fragmentPrefabs.Count == 0)
            {
                Debug.LogWarning("DebrisClusterManager: no fragment prefabs assigned.");
                return;
            }

            Transform parentForSpawn = spawnParent != null ? spawnParent : transform;

            for (int i = 0; i < fragmentCount; i++)
            {
                GameObject prefab = fragmentPrefabs[Random.Range(0, fragmentPrefabs.Count)];
                GameObject newFrag = Instantiate(prefab, parentForSpawn);

                var visual = newFrag.GetComponent<SimulatedOrbitVisual>();
                if (visual == null) visual = newFrag.AddComponent<SimulatedOrbitVisual>();

                visual.semiMajorAxisMeters = baseSemiMajorAxis + Random.Range(-axisRandomRange, axisRandomRange);
                visual.semiMinorAxisMeters = visual.semiMajorAxisMeters * Random.Range(0.75f, 0.95f);
                visual.inclinationDegrees = Random.Range(-inclinationRandomRangeDegrees, inclinationRandomRangeDegrees);
                visual.periodSeconds = periodBaseSeconds + Random.Range(-periodRandomRange, periodRandomRange);
                visual.phaseOffset = Random.Range(0f, 1f);

                Rigidbody rb = newFrag.GetComponent<Rigidbody>();
                if (rb != null) Destroy(rb);

                activeFragments.Add(newFrag);
            }

            if (untrackedHaze != null) untrackedHaze.Play();

            Debug.Log($"DebrisClusterManager: Generated {fragmentCount} simulated fragments.");
        }
    }
}