using UnityEngine;
using OrbitGuard.Core;
using OrbitGuard.UI; 

namespace OrbitGuard.Managers
{
    public class CollisionOutcomeManager : MonoBehaviour
    {
        [Header("Physical Meshes")]
        public Transform satelliteMesh; 
        public Transform debrisMesh;    

        [Header("Collision Parameters")]
        public double tcaSeconds;
        public float collisionThresholdUnits = 0.05f;

        private bool hasTriggeredOutcome = false;

        private void Update()
        {
            if (TimeController.Instance == null || satelliteMesh == null || debrisMesh == null) return;

            double currentTime = TimeController.Instance.SimulationTime;
            float currentDistance = Vector3.Distance(satelliteMesh.position, debrisMesh.position);

            // 1. LIVE HUD UPDATES
            if (WristHUDController.Instance != null)
            {
                WristHUDController.Instance.UpdateSimTime((float)currentTime);
                WristHUDController.Instance.UpdateMissDistance(currentDistance * 1000f); 
            }

            // 2. TIME-TRAVEL STATE MACHINE
            bool isPastTca = currentTime > tcaSeconds + 5.0; // 5-second buffer past closest approach

            if (isPastTca && !hasTriggeredOutcome)
            {
                TriggerOutcome(currentDistance); // We just scrubbed forward into the crash
            }
            else if (!isPastTca && hasTriggeredOutcome)
            {
                ResetToPreEncounter(); // We just rewound time back to safety!
            }
        }

        private void TriggerOutcome(float finalDistance)
        {
            hasTriggeredOutcome = true;
            bool collided = finalDistance < collisionThresholdUnits;

            if (WristHUDController.Instance != null)
            {
                WristHUDController.Instance.UpdateHeader(
                    collided ? "IMPACT DETECTED" : "SAFE PASSAGE", 
                    collided ? Color.red : Color.green
                );
            }

            if (collided)
            {
                Debug.Log($"[Collision] CATASTROPHIC IMPACT! Distance: {finalDistance:F4}");
                // Hide the satellite's 3D models to simulate destruction
                SetMeshVisibility(satelliteMesh, false);
            }
            else
            {
                Debug.Log($"[Collision] SAFE. Distance: {finalDistance:F4}");
            }
        }

        private void ResetToPreEncounter()
        {
            hasTriggeredOutcome = false;

            // Heal the satellite because we rewound time
            SetMeshVisibility(satelliteMesh, true);

            if (WristHUDController.Instance != null)
            {
                WristHUDController.Instance.UpdateHeader("STATUS: NOMINAL", Color.cyan);
            }
            
            Debug.Log("CollisionOutcomeManager: Time rewound. Satellite restored to nominal state.");
        }

        // Toggles visibility without destroying the GameObject (which would break the orbits)
        private void SetMeshVisibility(Transform targetNode, bool isVisible)
        {
            if (targetNode == null) return;
            
            MeshRenderer[] renderers = targetNode.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in renderers)
            {
                r.enabled = isVisible;
            }
        }

        // --- THE MISSING METHOD ---
        // Called by ConjunctionManager when loading a new file
        public void ResetOutcome(double newTcaSeconds)
        {
            tcaSeconds = newTcaSeconds;
            hasTriggeredOutcome = false;
            
            // Ensure satellite is visible on load
            SetMeshVisibility(satelliteMesh, true);

            if (WristHUDController.Instance != null)
            {
                WristHUDController.Instance.UpdateHeader("STATUS: NOMINAL", Color.cyan);
            }
        }
    }
}