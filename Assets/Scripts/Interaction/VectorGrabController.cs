// REVERTS to real orbital mechanics — grabbing perturbs the ACTUAL
// OrbitalElements on Orbit_Satellite_Macro (via Initialize), which
// re-propagates the real Keplerian path and moves the attached
// Satellite_Mesh with it. No UI dependency — Pc/fuel feedback goes to the
// Console log for now, per the current UI-free setup.

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using OrbitGuard.Core;

namespace OrbitGuard.Interaction
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class VectorGrabController : MonoBehaviour
    {
        [Tooltip("Orbit_Satellite_Macro's OrbitPropagator.")]
        public OrbitPropagator satelliteOrbitPropagator;

        public ThrusterModule thrusterVisual;
        public Transform zeroDeltaVAnchor;

        public float metersPerMps = 0.05f;
        public float maxDeltaVPerAxisMps = 2.0f;

        private XRGrabInteractable grabInteractable;
        private bool isHeld;
        private OrbitalElements baselineElements; // captured on grab, so dragging is fully reversible instead of compounding frame over frame

        private void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
        }

        private void OnEnable()
        {
            grabInteractable.selectEntered.AddListener(OnGrabBegin);
            grabInteractable.selectExited.AddListener(OnGrabEnd);
        }

        private void OnDisable()
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabBegin);
            grabInteractable.selectExited.RemoveListener(OnGrabEnd);
        }

        private void OnGrabBegin(SelectEnterEventArgs args)
        {
            isHeld = true;
            if (satelliteOrbitPropagator != null)
                baselineElements = satelliteOrbitPropagator.currentElements;
        }

        private void OnGrabEnd(SelectExitEventArgs args)
        {
            isHeld = false;
        }

        private void Update()
        {
            if (!isHeld || zeroDeltaVAnchor == null || satelliteOrbitPropagator == null) return;

            Vector3 displacementMeters = transform.localPosition - zeroDeltaVAnchor.localPosition;
            Vector3 deltaVMps = displacementMeters / Mathf.Max(metersPerMps, 0.001f);

            deltaVMps.x = Mathf.Clamp(deltaVMps.x, -maxDeltaVPerAxisMps, maxDeltaVPerAxisMps);
            deltaVMps.y = Mathf.Clamp(deltaVMps.y, -maxDeltaVPerAxisMps, maxDeltaVPerAxisMps);
            deltaVMps.z = Mathf.Clamp(deltaVMps.z, -maxDeltaVPerAxisMps, maxDeltaVPerAxisMps);

            OrbitalElements updated = ApplyDeltaVApproximation(baselineElements, deltaVMps);
            satelliteOrbitPropagator.Initialize(updated);

            if (thrusterVisual != null && deltaVMps.magnitude > 0.02f)
                thrusterVisual.FireThruster();
        }

        private OrbitalElements ApplyDeltaVApproximation(OrbitalElements baseline, Vector3 deltaVRic)
        {
            const double radialSensitivity = 8.0;
            const double inTrackSensitivity = 15.0;
            const double crossTrackSensitivity = 0.002;

            OrbitalElements updated = baseline;
            updated.semiMajorAxis += deltaVRic.x * radialSensitivity + deltaVRic.z * inTrackSensitivity;
            updated.inclination += deltaVRic.y * crossTrackSensitivity;
            return updated;
        }
    }
}