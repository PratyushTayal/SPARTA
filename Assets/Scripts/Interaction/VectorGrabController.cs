using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using OrbitGuard.Core;
using OrbitGuard.Managers;
using OrbitGuard.Data;

namespace OrbitGuard.Interaction
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class VectorGrabController : MonoBehaviour
    {
        [Header("References")]
        public OrbitPropagator satelliteOrbitPropagator;
        public OrbitPropagator debrisOrbitPropagator;
        public Transform zeroDeltaVAnchor;
        public ThrusterModule thrusterVisual;

        [Header("The Active Conjunction")]
        public ConjunctionData activeCdm;

        [Header("Tuning")]
        public float metersPerMps = 0.05f;
        public float maxDeltaVPerAxisMps = 2.0f;

        private XRGrabInteractable grabInteractable;
        private bool isHeld;
        private Vector3 currentDeltaVRic; 

        public Vector3 CurrentDeltaV => currentDeltaVRic;

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
            if (TelemetryStateManager.Instance != null)
                TelemetryStateManager.Instance.BeginCounterfactualExploration();
        }

        private void OnGrabEnd(SelectExitEventArgs args)
        {
            isHeld = false;
        }

        private void Update()
        {
            if (!isHeld || zeroDeltaVAnchor == null) return;

            Vector3 displacementMeters = transform.localPosition - zeroDeltaVAnchor.localPosition;
            Vector3 deltaVMps = displacementMeters / Mathf.Max(metersPerMps, 0.001f);

            deltaVMps.x = Mathf.Clamp(deltaVMps.x, -maxDeltaVPerAxisMps, maxDeltaVPerAxisMps);
            deltaVMps.y = Mathf.Clamp(deltaVMps.y, -maxDeltaVPerAxisMps, maxDeltaVPerAxisMps);
            deltaVMps.z = Mathf.Clamp(deltaVMps.z, -maxDeltaVPerAxisMps, maxDeltaVPerAxisMps);

            currentDeltaVRic = deltaVMps;
            ApplyToCounterfactualBranch(currentDeltaVRic);
        }

        private void ApplyToCounterfactualBranch(Vector3 deltaVRic)
        {
            var tsm = TelemetryStateManager.Instance;
            if (tsm == null || satelliteOrbitPropagator == null) return;

            OrbitalElements baseline = tsm.LiveTelemetry;
            OrbitalElements updated = ApplyDeltaVApproximation(baseline, deltaVRic);

            tsm.CounterfactualTelemetry = updated;
            satelliteOrbitPropagator.Initialize(updated); 

            RecomputeRiskLive(updated);

            if (thrusterVisual != null && deltaVRic.magnitude > 0.02f)
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

        private void RecomputeRiskLive(OrbitalElements updatedPrimaryElements)
        {
            if (RiskManager.Instance == null || debrisOrbitPropagator == null) return;

            RiskManager.Instance.RecomputeFromLivePositions(
                activeCdm,
                updatedPrimaryElements,
                debrisOrbitPropagator.currentElements,
                activeCdm.tcaSeconds);
        }
    }
}