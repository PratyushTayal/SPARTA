using UnityEngine;
using OrbitGuard.Core;
using OrbitGuard.UI;

namespace OrbitGuard.Managers
{
    public class CollisionOutcomeManager : MonoBehaviour
    {
        [Header("Real Orbital Data")]
        public OrbitPropagator satelliteOrbitPropagator;
        public OrbitPropagator debrisOrbitPropagator;

        [Header("Physical Meshes (for visual effects/destruction only)")]
        public Transform satelliteMesh;
        public Transform debrisMesh;
        
        [Header("Extra Cleanup")]
        [Tooltip("Drag Ghost Lines or UI here to hide them when the ship explodes.")]
        public GameObject[] hideOnImpact;

        [Header("Collision Parameters")]
        public double tcaSeconds;
        public float collisionThresholdUnits = 0.05f;
        public int subSamplesPerFrame = 30;

        private bool hasTriggeredOutcome = false;
        private double previousSimTime;
        private bool hasPreviousSimTime = false;

        private void Update()
        {
            if (TimeController.Instance == null || satelliteOrbitPropagator == null || debrisOrbitPropagator == null) return;

            double currentSimTime = TimeController.Instance.SimulationTime;

            if (!hasPreviousSimTime)
            {
                previousSimTime = currentSimTime;
                hasPreviousSimTime = true;
            }

            float minDistanceThisStep = FindMinimumDistanceAcrossStep(previousSimTime, currentSimTime);

            if (WristHUDController.Instance != null)
            {
                WristHUDController.Instance.UpdateSimTime((float)currentSimTime);
                WristHUDController.Instance.UpdateMissDistance(minDistanceThisStep * 1000f);
            }

            bool isPastTca = currentSimTime > tcaSeconds + 5.0;
            bool wasPastTca = previousSimTime > tcaSeconds + 5.0;

            if (isPastTca && !wasPastTca && !hasTriggeredOutcome)
            {
                TriggerOutcome(minDistanceThisStep);
            }
            else if (!isPastTca && hasTriggeredOutcome)
            {
                ResetToPreEncounter();
            }

            previousSimTime = currentSimTime;
        }

        private float FindMinimumDistanceAcrossStep(double startTime, double endTime)
        {
            double lo = System.Math.Min(startTime, endTime);
            double hi = System.Math.Max(startTime, endTime);

            float minDistance = float.MaxValue;
            int samples = System.Math.Max(2, subSamplesPerFrame);
            
            for (int i = 0; i < samples; i++)
            {
                double t = lo + (hi - lo) * (i / (double)(samples - 1));

                Vector3 satPos = KeplerianMath.GetPosition(satelliteOrbitPropagator.currentElements, t);
                Vector3 debPos = KeplerianMath.GetPosition(debrisOrbitPropagator.currentElements, t);
                float dist = Vector3.Distance(satPos, debPos) / ScaleConstants.KmPerMacroUnit;

                if (dist < minDistance) minDistance = dist;
            }

            return minDistance;
        }

        private void TriggerOutcome(float minDistanceFound)
        {
            hasTriggeredOutcome = true;
            bool collided = minDistanceFound < collisionThresholdUnits;

            if (WristHUDController.Instance != null)
            {
                WristHUDController.Instance.UpdateHeader(
                    collided ? " IMPACT DETECTED" : " SAFE PASSAGE",
                    collided ? Color.red : Color.green
                );
            }

            if (collided)
            {
                Debug.Log($"[Collision] CATASTROPHIC IMPACT! True minimum distance found: {minDistanceFound:F5} units.");
                SetSatelliteVisibility(false);
                ToggleExtras(false);
            }
            else
            {
                Debug.Log($"[Collision] SAFE. True minimum distance found: {minDistanceFound:F5} units.");
            }
        }

        private void ResetToPreEncounter()
        {
            hasTriggeredOutcome = false;
            SetSatelliteVisibility(true);
            ToggleExtras(true);

            if (WristHUDController.Instance != null)
                WristHUDController.Instance.UpdateHeader(" STATUS: NOMINAL", Color.cyan);

            Debug.Log("CollisionOutcomeManager: Time rewound. Satellite restored to nominal state.");
        }

        // --- THE UPGRADED VISIBILITY FUNCTION ---
        private void SetSatelliteVisibility(bool isVisible)
        {
            // 1. Toggle the 3D Model
            if (satelliteMesh != null)
            {
                MeshRenderer[] renderers = satelliteMesh.GetComponentsInChildren<MeshRenderer>(true);
                foreach (var r in renderers) r.enabled = isVisible;
            }

            // 2. Toggle the Orbit Line Ring
            if (satelliteOrbitPropagator != null)
            {
                LineRenderer lr = satelliteOrbitPropagator.GetComponent<LineRenderer>();
                if (lr != null) lr.enabled = isVisible;
            }
        }

        private void ToggleExtras(bool isVisible)
        {
            if (hideOnImpact == null) return;
            foreach (var obj in hideOnImpact)
            {
                if (obj != null) obj.SetActive(isVisible);
            }
        }

        public void ResetOutcome(double newTcaSeconds)
        {
            tcaSeconds = newTcaSeconds;
            hasTriggeredOutcome = false;
            hasPreviousSimTime = false; 
            SetSatelliteVisibility(true);
            ToggleExtras(true);

            if (WristHUDController.Instance != null)
                WristHUDController.Instance.UpdateHeader(" STATUS: NOMINAL", Color.cyan);
        }
    }
}