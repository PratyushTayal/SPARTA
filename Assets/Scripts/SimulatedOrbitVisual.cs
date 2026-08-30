using UnityEngine;

namespace OrbitGuard.Core
{
    public class SimulatedOrbitVisual : MonoBehaviour
    {
        [Header("Ellipse Shape (local Unity meters — NOT km)")]
        public float semiMajorAxisMeters = 2.0f;
        public float semiMinorAxisMeters = 1.6f;
        public float inclinationDegrees = 15f;

        [Header("Timing")]
        public float periodSeconds = 30f;
        [Range(0f, 1f)]
        public float phaseOffset = 0f;

        [Header("Live Burn Offset (written by VectorGrabController)")]
        public Vector3 burnOffsetMeters = Vector3.zero;

        private void Update()
        {
            double simTime = TimeController.Instance != null ? TimeController.Instance.SimulationTime : Time.time;

            float t = (float)(simTime / periodSeconds) + phaseOffset;
            float theta = t * 2f * Mathf.PI;

            float x = semiMajorAxisMeters * Mathf.Cos(theta);
            float z = semiMinorAxisMeters * Mathf.Sin(theta);

            Vector3 flatPosition = new Vector3(x, 0f, z);
            Vector3 tiltedPosition = Quaternion.Euler(inclinationDegrees, 0f, 0f) * flatPosition;

            transform.localPosition = tiltedPosition + burnOffsetMeters;
        }

        public Vector3 GetBasePositionAtNormalizedTime(float normalizedTime)
        {
            float theta = normalizedTime * 2f * Mathf.PI;
            float x = semiMajorAxisMeters * Mathf.Cos(theta);
            float z = semiMinorAxisMeters * Mathf.Sin(theta);
            Vector3 flatPosition = new Vector3(x, 0f, z);
            return Quaternion.Euler(inclinationDegrees, 0f, 0f) * flatPosition;
        }
    }
}