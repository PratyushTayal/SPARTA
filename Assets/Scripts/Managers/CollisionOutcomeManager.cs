// NEW FILE — nothing before this ever resolved a conjunction into an
// actual outcome. This watches the real distance between the two macro
// objects as simulation time crosses TCA and fires a clear result.

using UnityEngine;
using OrbitGuard.Core;

namespace OrbitGuard.Managers
{
    public class CollisionOutcomeManager : MonoBehaviour
    {
        public Transform satelliteMesh; // the real Satellite_Mesh, parented under Orbit_Satellite_Macro
        public Transform debrisMesh;    // the real Debris_Mesh, parented under Orbit_Debris_Macro

        [Tooltip("Real TCA, seconds — same value ConjunctionManager parsed from the CDM.")]
        public double tcaSeconds;

        [Tooltip("Collision threshold, Unity units (macro scale — 1 unit = 1000km via ScaleConstants.KmPerMacroUnit, so this should be your combined hard-body radius converted to that scale, or exaggerated for visibility since a real ~20m HBR is far too small to ever visually register at macro scale).")]
        public float collisionThresholdUnits = 0.05f;

        public GameObject collisionVfxPrefab; // simple burst/explosion, spawned at the point of closest approach
        public AudioSource collisionAudio;
        public AudioSource safePassageAudio;

        private bool outcomeResolved = false;

        private void Update()
        {
            if (outcomeResolved || TimeController.Instance == null) return;
            if (satelliteMesh == null || debrisMesh == null) return;

            // Resolve shortly after TCA passes, once the closest-approach
            // moment has actually happened — not exactly AT tcaSeconds,
            // since exact frame timing will never land precisely on it.
            if (TimeController.Instance.SimulationTime > tcaSeconds + 60.0)
            {
                float distance = Vector3.Distance(satelliteMesh.position, debrisMesh.position);
                ResolveOutcome(distance);
            }
        }

        private void ResolveOutcome(float finalDistance)
        {
            outcomeResolved = true;

            bool collided = finalDistance < collisionThresholdUnits;

            if (collided)
            {
                Debug.Log($"CollisionOutcomeManager: COLLISION — final separation {finalDistance:F4} units, threshold {collisionThresholdUnits}.");
                if (collisionVfxPrefab != null)
                    Instantiate(collisionVfxPrefab, satelliteMesh.position, Quaternion.identity);
                if (collisionAudio != null)
                    collisionAudio.Play();
            }
            else
            {
                Debug.Log($"CollisionOutcomeManager: SAFE PASSAGE — final separation {finalDistance:F4} units, threshold {collisionThresholdUnits}.");
                if (safePassageAudio != null)
                    safePassageAudio.Play();
            }
        }

        /// <summary>Call this from ConjunctionManager after loading a new CDM, or from a "retry" button, to re-arm the check for a fresh run.</summary>
        public void ResetOutcome(double newTcaSeconds)
        {
            tcaSeconds = newTcaSeconds;
            outcomeResolved = false;
        }
    }
}