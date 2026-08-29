using UnityEngine;
using System.Collections.Generic;
using OrbitGuard.Core;

namespace OrbitGuard.Managers
{
    public class DebrisClusterManager : MonoBehaviour
    {
        public static DebrisClusterManager Instance { get; private set; }

        [Tooltip("The small cube prefab representing a fragment")]
        public GameObject fragmentPrefab;

        [Tooltip("Drag the [DebrisCluster] GameObject here")]
        public Transform spawnParent;

        [Tooltip("Drag the Placeholder_Satellite here so fragments know what to scale against")]
        public OrbitPropagator primaryPropagator;

        [Tooltip("How many fragments to simulate (15-30 recommended)")]
        public int fragmentCount = 20;

        [Tooltip("The particle system representing the thousands of untracked fragments")]
        public ParticleSystem untrackedHaze;

        private List<GameObject> activeFragments = new List<GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void GenerateCluster(OrbitalElements parentElements)
        {
            // Clear old fragments if any
            foreach (var frag in activeFragments) Destroy(frag);
            activeFragments.Clear();

            for (int i = 0; i < fragmentCount; i++)
            {
                // 1. Statistically perturb the parent orbit to simulate the breakup explosion
                OrbitalElements fragElements = parentElements;

                // Semi-major axis varies by a few kilometers (Added 'f' suffixes here!)
                fragElements.semiMajorAxis += UnityEngine.Random.Range(-15.0f, 15.0f); 
                
                // Inclination and RAAN vary by tiny fractions to create a cone/band
                fragElements.inclination += UnityEngine.Random.Range(-0.002f, 0.002f);
                fragElements.raan += UnityEngine.Random.Range(-0.01f, 0.01f);
                
                // Spread them out along the orbit path so they aren't clumped in one dot
                fragElements.meanAnomalyAtEpoch += UnityEngine.Random.Range(-0.1f, 0.1f);

                // 2. Instantiate the physical fragment under the designated spawn parent
                Transform parentTransform = spawnParent != null ? spawnParent : this.transform;
                GameObject newFrag = Instantiate(fragmentPrefab, parentTransform);
                
                // 3. Hook up its math engine
                OrbitPropagator prop = newFrag.GetComponent<OrbitPropagator>();
                if (prop != null)
                {
                    prop.Initialize(fragElements);
                    
                    // FIXED: Removed "OrbitPropagator." prefix from OrbitDisplayMode
                    prop.displayMode = OrbitDisplayMode.EncounterRelative; 
                    prop.relativeReference = primaryPropagator;
                    
                    // Make fragment lines thinner and dimmer than the main satellites
                    LineRenderer lr = newFrag.GetComponent<LineRenderer>();
                    if (lr != null)
                    {
                        lr.startWidth = 0.005f;
                        lr.endWidth = 0.005f;
                        // Faint orange color for debris
                        lr.startColor = new Color(1f, 0.4f, 0f, 0.3f); 
                        lr.endColor = new Color(1f, 0.4f, 0f, 0.3f);
                    }
                }

                activeFragments.Add(newFrag);
            }

            // Turn on the visual haze for the 1,500 untracked pieces
            if (untrackedHaze != null) untrackedHaze.Play();
            
            Debug.Log($"DebrisClusterManager: Generated {fragmentCount} tracked fragments and activated untracked haze.");
        }
    }
}