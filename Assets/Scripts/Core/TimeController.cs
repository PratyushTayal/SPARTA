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

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            if (IsPlaying)
            {
                SimulationTime += Time.deltaTime * TimeScale;
            }
        }

        // NEW: Allows a UI button to pause/unpause the simulation
        public void TogglePlay() 
        { 
            IsPlaying = !IsPlaying; 
        }
    }
}