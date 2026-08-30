// REVERTS DebrisClusterManager back to real orbital math, matching your
// current architecture (Satellite_Mesh/Debris_Mesh parented directly to
// Orbit_Satellite_Macro/Orbit_Debris_Macro).
//
// THE FIX for "debris orbits the XR Origin": spawnParent must be an empty
// GameObject at Earth's local origin, a SIBLING of Orbit_Satellite_Macro/
// Orbit_Debris_Macro under [Macro_Space_Orbital_Deck] — NOT under XR
// Origin, and not left unassigned.

using UnityEngine;
using System.Collections.Generic;
using OrbitGuard.Core;

namespace OrbitGuard.Managers
{
    public class DebrisClusterManager : MonoBehaviour
    {
        public static DebrisClusterManager Instance { get; private set; }

        public GameObject fragmentPrefab;

        [Tooltip("MUST be an empty GameObject at Earth's local origin, sibling of Orbit_Satellite_Macro/Orbit_Debris_Macro. NOT under XR Origin.")]
        public Transform spawnParent;

        public int fragmentCount = 20;
        public ParticleSystem untrackedHaze;

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

            if (spawnParent == null)
            {
                Debug.LogError("DebrisClusterManager: spawnParent not assigned — fragments would spawn in the wrong hierarchy.");
                return;
            }

            for (int i = 0; i < fragmentCount; i++)
            {
                OrbitalElements fragElements = parentElements;
                // ADDED 'f' TO ALL NUMBERS HERE:
                fragElements.semiMajorAxis += Random.Range(-15.0f, 15.0f);
                fragElements.inclination += Random.Range(-0.02f, 0.02f);
                fragElements.raan += Random.Range(-0.05f, 0.05f);
                fragElements.meanAnomalyAtEpoch += Random.Range(-0.3f, 0.3f);

                GameObject newFrag = Instantiate(fragmentPrefab, spawnParent);

                OrbitPropagator prop = newFrag.GetComponent<OrbitPropagator>();
                if (prop != null)
                {
                    prop.displayMode = OrbitDisplayMode.Macro;
                    prop.Initialize(fragElements);

                    LineRenderer lr = newFrag.GetComponent<LineRenderer>();
                    if (lr != null)
                    {
                        lr.startWidth = 0.01f;
                        lr.endWidth = 0.01f;
                        lr.startColor = new Color(1f, 0.4f, 0f, 0.3f);
                        lr.endColor = new Color(1f, 0.4f, 0f, 0.3f);
                    }
                }

                Rigidbody rb = newFrag.GetComponent<Rigidbody>();
                if (rb != null) Destroy(rb);

                activeFragments.Add(newFrag);
            }

            if (untrackedHaze != null) untrackedHaze.Play();
            Debug.Log($"DebrisClusterManager: Generated {fragmentCount} real-math fragments around Earth.");
        }
    }
}