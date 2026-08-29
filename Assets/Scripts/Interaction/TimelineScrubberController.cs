using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using OrbitGuard.Core;
using OrbitGuard.Managers;
using OrbitGuard.Data;

namespace OrbitGuard.Interaction
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class TimelineScrubberController : MonoBehaviour
    {
        [Header("Rail Geometry")]
        [Tooltip("The two ends of the physical rail, local to the handle's parent — defines the min/max the handle can slide between.")]
        public Transform railStart;
        public Transform railEnd;

        [Header("Time Mapping")]
        [Tooltip("Simulation seconds represented by the rail's start (usually 0 = CDM epoch).")]
        public double timeAtRailStart = 0.0;

        [Tooltip("Simulation seconds represented by the rail's end — the bible's ~48-hour look-ahead window.")]
        public double timeAtRailEnd = 48.0 * 3600.0;

        [Header("References for Live Collision Preview")]
        public OrbitPropagator primaryOrbitPropagator;
        public OrbitPropagator debrisOrbitPropagator;
        public ConjunctionData activeCdm;

        [Header("Visual Feedback")]
        [Tooltip("Optional — a material/renderer whose color reflects current risk while scrubbing, e.g. the handle itself flashing red near TCA.")]
        public Renderer handleRenderer;
        public Color safeColor = new Color(0.18f, 0.8f, 0.44f);
        public Color dangerColor = new Color(0.91f, 0.3f, 0.24f);

        private XRGrabInteractable grabInteractable;
        private bool isHeld;
        private bool wasAutoPlaying;

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

            if (TimeController.Instance != null)
            {
                wasAutoPlaying = TimeController.Instance.IsPlaying;
                TimeController.Instance.IsPlaying = false;
            }

            if (TelemetryStateManager.Instance != null)
                TelemetryStateManager.Instance.BeginCounterfactualExploration();
        }

        private void OnGrabEnd(SelectExitEventArgs args)
        {
            isHeld = false;
            if (TimeController.Instance != null)
                TimeController.Instance.IsPlaying = wasAutoPlaying;
        }

        private void Update()
        {
            if (!isHeld || railStart == null || railEnd == null || TimeController.Instance == null) return;

            Vector3 railVector = railEnd.localPosition - railStart.localPosition;
            float railLength = railVector.magnitude;
            if (railLength < 0.0001f) return;

            Vector3 toHandle = transform.localPosition - railStart.localPosition;
            float projected = Vector3.Dot(toHandle, railVector.normalized);
            float clampedProjected = Mathf.Clamp(projected, 0f, railLength);
            float normalized = clampedProjected / railLength;

            transform.localPosition = railStart.localPosition + railVector.normalized * clampedProjected;

            double newSimTime = timeAtRailStart + normalized * (timeAtRailEnd - timeAtRailStart);
            TimeController.Instance.SimulationTime = newSimTime;

            UpdateLiveCollisionPreview(newSimTime);
        }

        private void UpdateLiveCollisionPreview(double simTime)
        {
            if (RiskManager.Instance == null || primaryOrbitPropagator == null || debrisOrbitPropagator == null) return;

            OrbitalElements primaryElements = TelemetryStateManager.Instance != null
                ? TelemetryStateManager.Instance.CounterfactualTelemetry
                : primaryOrbitPropagator.currentElements;

            RiskManager.Instance.RecomputeFromLivePositions(
                activeCdm,
                primaryElements,
                debrisOrbitPropagator.currentElements,
                simTime);

            if (handleRenderer != null)
            {
                double missKm = RiskManager.Instance.CurrentLiveMissDistanceKm;
                double combinedHbrKm = activeCdm.CombinedHardBodyRadiusMeters() / 1000.0;
                bool isDanger = missKm < combinedHbrKm * 10.0;
                handleRenderer.material.color = isDanger ? dangerColor : safeColor;
            }
        }
    }
}