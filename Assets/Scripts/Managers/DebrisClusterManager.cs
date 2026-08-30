using UnityEngine;
using System.Collections.Generic;
using OrbitGuard.Core;

namespace OrbitGuard.Managers
{
    public class DebrisClusterManager : MonoBehaviour
    {
        public static DebrisClusterManager Instance { get; private set; }

        [Header("Prefabs & References")]
        public List<GameObject> fragmentPrefabs;
        public Transform spawnParent;
        public ParticleSystem untrackedHaze;

        [Header("Cluster Settings")]
        public int fragmentCount = 100;

        [Tooltip("Base visual scale multiplier applied to the prefab's original size.")]
        [Range(0.1f, 50.0f)]
        public float fragmentVisualScale = 5.0f;
        
        [Tooltip("Minimum random size multiplier (e.g., 0.5 = half size)")]
        public float minSizeMultiplier = 0.3f;
        
        [Tooltip("Maximum random size multiplier (e.g., 2.0 = double size)")]
        public float maxSizeMultiplier = 2.5f;

        [Header("Orbit Scatter Settings")]
        [Tooltip("Keep this LOW (e.g. 0.5 - 2) so they stay in a tight shell around Earth.")]
        public float altitudeScatter = 1.5f;
        
        [Tooltip("Keep this moderate (e.g. 5-15) for a realistic debris band.")]
        public float inclinationScatter = 8f;
        
        [Tooltip("Keep this moderate (e.g. 5-15).")]
        public float raanScatter = 10f;
        
        [Tooltip("Use 360 to spread them completely around the ring.")]
        public float anomalyScatter = 360f;

        [Header("Orbit Line Visuals")]
        public bool showFragmentOrbitLines = false; 

        [Range(0.005f, 0.5f)]
        public float orbitLineWidth = 0.02f;

        private List<GameObject> activeFragments = new List<GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void GenerateCluster(OrbitalElements parentElements)
        {
            foreach (var frag in activeFragments) Destroy(frag);
            activeFragments.Clear();

            if (spawnParent == null || fragmentPrefabs == null || fragmentPrefabs.Count == 0)
            {
                Debug.LogError("DebrisClusterManager: Missing spawnParent or empty prefab list.");
                return;
            }

            for (int i = 0; i < fragmentCount; i++)
            {
                OrbitalElements fragElements = parentElements;
                
                // Keep the debris in a tighter shell, but spread them fully around the planet
                fragElements.semiMajorAxis += Random.Range(-altitudeScatter, altitudeScatter);
                fragElements.inclination += Random.Range(-inclinationScatter, inclinationScatter);
                fragElements.raan += Random.Range(-raanScatter, raanScatter);
                fragElements.meanAnomalyAtEpoch += Random.Range(-anomalyScatter, anomalyScatter);

                GameObject prefabToSpawn = fragmentPrefabs[Random.Range(0, fragmentPrefabs.Count)];
                GameObject newFrag = Instantiate(prefabToSpawn, spawnParent);

                // FIX: Respect original prefab scale, apply base scale, AND add random size variation
                float randomScaleFactor = Random.Range(minSizeMultiplier, maxSizeMultiplier);
                newFrag.transform.localScale = prefabToSpawn.transform.localScale * fragmentVisualScale * randomScaleFactor;

                OrbitPropagator prop = newFrag.GetComponent<OrbitPropagator>();
                if (prop != null)
                {
                    prop.displayMode = OrbitDisplayMode.Macro;
                    prop.Initialize(fragElements);

                    LineRenderer lr = newFrag.GetComponent<LineRenderer>();
                    if (lr != null)
                    {
                        lr.enabled = showFragmentOrbitLines;
                        if (showFragmentOrbitLines)
                        {
                            lr.startWidth = orbitLineWidth;
                            lr.endWidth = orbitLineWidth;
                            lr.startColor = new Color(1f, 0.4f, 0f, 0.15f); // Lowered alpha for less clutter
                            lr.endColor = new Color(1f, 0.4f, 0f, 0.15f);
                        }
                    }
                }

                Rigidbody rb = newFrag.GetComponent<Rigidbody>();
                if (rb != null) Destroy(rb);

                activeFragments.Add(newFrag);
            }

            if (untrackedHaze != null) untrackedHaze.Play();
            Debug.Log($"DebrisClusterManager: Generated {fragmentCount} scattered fragments.");
        }
    }
}