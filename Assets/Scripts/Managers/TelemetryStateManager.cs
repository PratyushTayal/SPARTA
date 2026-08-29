using UnityEngine;
using System;
using OrbitGuard.Data;
using OrbitGuard.Core;

namespace OrbitGuard.Managers
{
    public class TelemetryStateManager : MonoBehaviour
    {
        // This makes it a Singleton, easily accessible from anywhere
        public static TelemetryStateManager Instance { get; private set; }

        [Header("Timelines")]
        [Tooltip("The real-time, un-editable data from the CDM.")]
        public OrbitalElements LiveTelemetry;
        
        [Tooltip("The sandbox data for VR grabbing and timeline scrubbing.")]
        public OrbitalElements CounterfactualTelemetry;

        // Tracks if the user is currently manipulating the timeline or vectors
        public bool IsExploring { get; private set; }

        // Event that tells the UI when a maneuver is officially proposed
        public Action<OrbitalElements> OnManeuverProposed;

        private void Awake()
        {
            // Ensure only one of these exists in the scene
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Called when the CdmParser successfully reads a new data file.
        /// </summary>
        public void IngestNewTelemetry(OrbitalElements newData)
        {
            LiveTelemetry = newData;
            
            // If the user isn't actively messing with the simulation, 
            // update their sandbox to match reality.
            if (!IsExploring)
            {
                CounterfactualTelemetry = newData;
            }
            Debug.Log("TelemetryStateManager: New Live Telemetry Ingested.");
        }

        /// <summary>
        /// Call this the exact moment the user GRABS the timeline scrubber or the Delta-V arrow.
        /// </summary>
        public void BeginCounterfactualExploration()
        {
            if (IsExploring) return; // We are already in the sandbox

            IsExploring = true;
            
            // Deep copy the live data into the sandbox.
            // (Assuming OrbitalElements is a struct, the '=' operator does a perfect value copy).
            CounterfactualTelemetry = LiveTelemetry; 
            
            Debug.Log("TelemetryStateManager: Multiverse branched! Counterfactual exploration started.");
        }

        /// <summary>
        /// Call this when the user clicks the "Lock In" or "Confirm Maneuver" button.
        /// </summary>
        public void ProposeManeuver(OrbitalElements proposedElements)
        {
            Debug.Log("TelemetryStateManager: Maneuver Proposed and Logged!");
            
            // Trigger any UI updates (like turning the ghost line into a solid line)
            OnManeuverProposed?.Invoke(proposedElements);
            
            // Note: We DO NOT overwrite LiveTelemetry here. Real tools require approval first.
        }

        /// <summary>
        /// Call this if the user hits a "Cancel" button to wipe their VR changes.
        /// </summary>
        public void ResetExploration()
        {
            IsExploring = false;
            CounterfactualTelemetry = LiveTelemetry;
            Debug.Log("TelemetryStateManager: Sandbox wiped. Snapped back to Live Telemetry.");
        }
    }
}