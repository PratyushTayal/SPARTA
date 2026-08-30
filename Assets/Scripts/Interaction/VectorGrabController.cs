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
        [Tooltip("The satellite's SimulatedOrbitVisual — this script writes burnOffsetMeters on it.")]
        public SimulatedOrbitVisual satelliteVisual;

        public ThrusterModule thrusterVisual;

        [Tooltip("Transform marking the zero-burn rest position for the arrow.")]
        public Transform zeroDeltaVAnchor;

        [Header("The Active Conjunction — used only for its reportedCollisionProbability baseline")]
        public ConjunctionData activeCdm;

        [Header("Tuning")]
        public float metersPerMps = 0.05f;
        public float maxDeltaVPerAxisMps = 2.0f;

        [Tooltip("Same curve ParetoSolver uses: at this Δv magnitude (m/s), Pc reduction saturates (i.e. burning harder than this gives no further benefit in the simplified model).")]
        public float pcReductionSaturationMps = 2.5f;

        private XRGrabInteractable grabInteractable;
        private bool isHeld;
        private Vector3 currentDeltaV;

        public Vector3 CurrentDeltaV => currentDeltaV;

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
        }

        private void OnGrabEnd(SelectExitEventArgs args)
        {
            isHeld = false;
        }

        private void Update()
        {
            if (!isHeld || zeroDeltaVAnchor == null || satelliteVisual == null) return;

            Vector3 displacementMeters = transform.localPosition - zeroDeltaVAnchor.localPosition;
            Vector3 deltaVMps = displacementMeters / Mathf.Max(metersPerMps, 0.001f);

            deltaVMps.x = Mathf.Clamp(deltaVMps.x, -maxDeltaVPerAxisMps, maxDeltaVPerAxisMps);
            deltaVMps.y = Mathf.Clamp(deltaVMps.y, -maxDeltaVPerAxisMps, maxDeltaVPerAxisMps);
            deltaVMps.z = Mathf.Clamp(deltaVMps.z, -maxDeltaVPerAxisMps, maxDeltaVPerAxisMps);

            currentDeltaV = deltaVMps;

            satelliteVisual.burnOffsetMeters = displacementMeters;

            RecomputeSimplifiedRisk(deltaVMps.magnitude);

            if (thrusterVisual != null && deltaVMps.magnitude > 0.02f)
                thrusterVisual.FireThruster();
        }

        private void RecomputeSimplifiedRisk(float deltaVMagnitude)
        {
            if (RiskManager.Instance == null) return;

            float reductionFactor = 1f - Mathf.Clamp01(deltaVMagnitude / pcReductionSaturationMps);
            float predictedPc = (float)activeCdm.reportedCollisionProbability * reductionFactor;

            RiskManager.Instance.OnPcUpdated?.Invoke(predictedPc);
        }
    }
}