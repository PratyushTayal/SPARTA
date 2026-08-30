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

        [Range(0.1f, 50.0f)]
        public float fragmentVisualScale = 5.0f;
        public float minSizeMultiplier = 0.3f;
        public float maxSizeMultiplier = 2.5f;

        [Header("Orbit Scatter Settings")]
        public float altitudeScatter = 1.5f;
        public float inclinationScatter = 8f;
        public float raanScatter = 10f;
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

            if (spawnParent == null || fragmentPrefabs == null || fragmentPrefabs.Count == 0) return;

            for (int i = 0; i < fragmentCount; i++)
            {
                OrbitalElements fragElements = parentElements;
                fragElements.semiMajorAxis += Random.Range(-altitudeScatter, altitudeScatter);
                fragElements.inclination += Random.Range(-inclinationScatter, inclinationScatter);
                fragElements.raan += Random.Range(-raanScatter, raanScatter);
                fragElements.meanAnomalyAtEpoch += Random.Range(-anomalyScatter, anomalyScatter);

                GameObject prefabToSpawn = fragmentPrefabs[Random.Range(0, fragmentPrefabs.Count)];
                GameObject newFrag = Instantiate(prefabToSpawn, spawnParent);

                OrbitPropagator prop = newFrag.GetComponent<OrbitPropagator>();
                if (prop != null)
                {
                    // 1. Keep the path anchor at scale 1 so the orbit line doesn't distort
                    newFrag.transform.localScale = Vector3.one;

                    // 2. Auto-Fix Hierarchy: Separate mesh from orbit path
                    Transform childVisual = null;
                    MeshRenderer rootMR = newFrag.GetComponent<MeshRenderer>();
                    
                    if (rootMR != null) 
                    {
                        GameObject visualObj = new GameObject("VisualBody");
                        visualObj.transform.SetParent(newFrag.transform, false);
                        
                        MeshFilter rootMF = newFrag.GetComponent<MeshFilter>();
                        if (rootMF != null) 
                        {
                            visualObj.AddComponent<MeshFilter>().sharedMesh = rootMF.sharedMesh;
                            Destroy(rootMF);
                        }
                        
                        MeshRenderer childMR = visualObj.AddComponent<MeshRenderer>();
                        childMR.sharedMaterials = rootMR.sharedMaterials;
                        Destroy(rootMR);
                        
                        childVisual = visualObj.transform;
                    }
                    else if (newFrag.transform.childCount > 0)
                    {
                        childVisual = newFrag.transform.GetChild(0);
                    }

                    // 3. Assign visual body and scale it
                    if (childVisual != null)
                    {
                        prop.visualBody = childVisual;
                        float randomScaleFactor = Random.Range(minSizeMultiplier, maxSizeMultiplier);
                        childVisual.localScale = prefabToSpawn.transform.localScale * fragmentVisualScale * randomScaleFactor;
                    }

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
                            lr.startColor = new Color(1f, 0.4f, 0f, 0.15f);
                            lr.endColor = new Color(1f, 0.4f, 0f, 0.15f);
                        }
                    }
                }

                Rigidbody rb = newFrag.GetComponent<Rigidbody>();
                if (rb != null) Destroy(rb);

                activeFragments.Add(newFrag);
            }
        }
    }
}