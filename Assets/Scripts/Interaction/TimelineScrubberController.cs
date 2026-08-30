using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using OrbitGuard.Core;
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
        public double timeAtRailStart = 0.0;
        public double timeAtRailEnd = 48.0 * 3600.0;

        [Header("References for Live Collision Preview")]
        public Transform primaryTransform;
        public Transform debrisTransform;
        public ConjunctionData activeCdm; // Kept so ConjunctionManager can still push data to it safely

        [Header("Visual Feedback")]
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

            UpdateLiveCollisionPreview();
        }

        private void UpdateLiveCollisionPreview()
        {
            if (primaryTransform == null || debrisTransform == null) return;

            // The simplified visual check
            float liveDistance = Vector3.Distance(primaryTransform.position, debrisTransform.position);
            bool isDanger = liveDistance < 0.3f; // Tune this threshold in the Inspector if needed
            
            if (handleRenderer != null)
            {
                handleRenderer.material.color = isDanger ? dangerColor : safeColor;
            }
        }
    }
}