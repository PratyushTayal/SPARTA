using UnityEngine;
using OrbitGuard.Core; // Ensure this matches your TimeController namespace

namespace OrbitGuard.Interaction
{
    public class PhysicalTimeController : MonoBehaviour
    {
        [Header("Speed Settings")]
        public float normalSpeed = 1000f;
        public float fastForwardSpeed = 5000f;
        public float rewindSpeed = -5000f;

        // Called by the Rewind Button
        public void OnRewindPressed()
        {
            if (TimeController.Instance == null) return;
            TimeController.Instance.IsPlaying = true;
            
            // Assuming your variable is named TimeScale. Adjust if it's timeScale or timeMultiplier!
            TimeController.Instance.TimeScale = rewindSpeed; 
            Debug.Log("Time: REWIND");
        }

        // Called by the Play/Pause Button
        public void OnPlayPausePressed()
        {
            if (TimeController.Instance == null) return;

            TimeController.Instance.IsPlaying = !TimeController.Instance.IsPlaying;
            
            if (TimeController.Instance.IsPlaying)
            {
                TimeController.Instance.TimeScale = normalSpeed;
            }
            Debug.Log($"Time: {(TimeController.Instance.IsPlaying ? "PLAY" : "PAUSED")}");
        }

        // Called by the Fast Forward Button
        public void OnFastForwardPressed()
        {
            if (TimeController.Instance == null) return;
            TimeController.Instance.IsPlaying = true;
            TimeController.Instance.TimeScale = fastForwardSpeed;
            Debug.Log("Time: FAST FORWARD");
        }
    }
}