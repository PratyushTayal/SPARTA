using UnityEngine;
using OrbitGuard.Data;
using OrbitGuard.Core;

namespace OrbitGuard.Rendering
{
    public class CovarianceBubbleController : MonoBehaviour
    {
        [Tooltip("The raw matrix from the CDM")]
        public CovarianceMatrix baseCovariance;
        
        [Tooltip("When was this data issued?")]
        public double cdmEpochSeconds;
        
        [Tooltip("How fast uncertainty grows per day (visual representation)")]
        public float growthRatePerDay = 1.15f; 

        void Update()
        {
            if (TimeController.Instance == null) return;

            // Calculate days passed since the NASA alert was issued
            double timePassedSeconds = TimeController.Instance.SimulationTime - cdmEpochSeconds;
            double daysPassed = Mathf.Max(0f, (float)(timePassedSeconds / 86400.0));

            // Grow the bubble over time
            float multiplier = 1.0f + (float)(daysPassed * growthRatePerDay);

            // Apply the matrix diagonals to the 3D scale, factored by our growth rate
            // Note: In real life, these numbers are tiny, so we scale them relative to the compressed Encounter Sphere
            float scaleFactor = 1000f / ScaleConstants.EncounterSphereCompressionFactor;
            float scaleX = (float)baseCovariance.crR * multiplier * scaleFactor;
            float scaleY = (float)baseCovariance.cnN * multiplier * scaleFactor;
            float scaleZ = (float)baseCovariance.ctT * multiplier * scaleFactor;

            transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        }
    }
}