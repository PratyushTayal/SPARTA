using UnityEngine;
using TMPro;

namespace OrbitGuard.UI
{
    public class WristHUDController : MonoBehaviour
    {
        public static WristHUDController Instance { get; private set; }

        [Header("Miss Distance (Panel 1)")]
        [Tooltip("Drag 'Panel1 -> CollisionProb' here")]
        public TextMeshProUGUI missDistanceLabel;
        [Tooltip("Drag 'Panel1 -> ColProbText' here")]
        public TextMeshProUGUI missDistanceValue;

        [Header("Simulation Time (Panel 1 (1))")]
        [Tooltip("Drag 'Panel1 (1) -> CollisionProb' here")]
        public TextMeshProUGUI simTimeLabel;
        [Tooltip("Drag 'Panel1 (1) -> ColProbText' here")]
        public TextMeshProUGUI simTimeValue;

        [Header("Header Text")]
        [Tooltip("Drag 'OrbitalSafety' here")]
        public TextMeshProUGUI headerText;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // Call this from your RiskManager or ConjunctionManager
        public void UpdateMissDistance(float distanceInKm)
        {
            if (missDistanceValue != null)
            {
                // Formats to 3 decimal places (e.g., 0.584 Km)
                missDistanceValue.text = $"{distanceInKm:F3} Km";
            }
        }

        // Call this from your TimeController
        public void UpdateSimTime(float timeInSeconds)
        {
            if (simTimeValue != null)
            {
                // Rounds to a whole number (e.g., 638 secs)
                simTimeValue.text = $"{Mathf.FloorToInt(timeInSeconds)} secs";
            }
        }

        // Optional: Update the "HELLO GANG" text to show status warnings
        public void UpdateHeader(string message, Color color)
        {
            if (headerText != null)
            {
                headerText.text = message;
                headerText.color = color;
            }
        }
    }
}