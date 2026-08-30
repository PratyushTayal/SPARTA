using UnityEngine;

namespace OrbitGuard.Core
{
    public class TimeController : MonoBehaviour
    {
        public static TimeController Instance { get; private set; }
        
        [Tooltip("Current time in seconds since the simulation started")]
        public double SimulationTime = 0;
        
        [Tooltip("Is the timeline playing automatically?")]
        public bool IsPlaying = true;
        
        [Tooltip("How fast time moves (1 = real time, 10 = fast forward)")]
        public float TimeScale = 10f; // Set to 10 by default now

        [Header("Automatic Slowdown Near Critical Events (fixes fast-forward skipping past close encounters)")]
        [Tooltip("Set this to the active conjunction's TCA (ConjunctionManager should assign it) so fast-forward automatically slows down near the moment of closest approach instead of potentially jumping past it entirely.")]
        public double criticalEventTimeSeconds = -1; // -1 = no critical event registered, no slowdown applied

        [Tooltip("How many seconds before/after the critical event the slowdown zone extends.")]
        public double criticalWindowSeconds = 300.0;

        [Tooltip("TimeScale is multiplied by this factor inside the critical window — e.g. 0.02 turns a 5000x fast-forward into an effective 100x, fine enough resolution to not skip over a fast-closing encounter.")]
        public float criticalSlowdownFactor = 0.02f;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            if (IsPlaying)
            {
                float effectiveScale = TimeScale;

                if (criticalEventTimeSeconds >= 0)
                {
                    double distanceFromEvent = System.Math.Abs(SimulationTime - criticalEventTimeSeconds);
                    if (distanceFromEvent <= criticalWindowSeconds)
                        effectiveScale *= criticalSlowdownFactor;
                }

                SimulationTime += Time.deltaTime * effectiveScale;
            }
        }

        // NEW: Allows a UI button to pause/unpause the simulation
        public void TogglePlay() 
        { 
            IsPlaying = !IsPlaying; 
        }
    }
}