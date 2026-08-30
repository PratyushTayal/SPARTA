using UnityEngine;

namespace OrbitGuard.Core
{
    public enum OrbitDisplayMode
    {
        Macro,
        EncounterRelative
    }

    [RequireComponent(typeof(LineRenderer))]
    public class OrbitPropagator : MonoBehaviour
    {
        public OrbitalElements currentElements;

        [Header("Movement (The Fix)")]
        [Tooltip("Assign the visual mesh here (e.g., Satellite_Mesh). The root stays anchored to Earth, and this child moves along the orbit.")]
        public Transform visualBody;

        [Header("Display Mode")]
        public OrbitDisplayMode displayMode = OrbitDisplayMode.Macro;
        public OrbitPropagator relativeReference;

        public int resolution = 128;
        private LineRenderer lineRenderer;

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = resolution;
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = (displayMode == OrbitDisplayMode.Macro);
        }

        public void Initialize(OrbitalElements realDataElements)
        {
            currentElements = realDataElements;
            DrawOrbit();
        }

        void Update()
        {
            if (currentElements.semiMajorAxis <= 0) return;

            double t = TimeController.Instance != null ? (currentElements.epoch + TimeController.Instance.SimulationTime) : currentElements.epoch;
            Vector3 displayPos = GetDisplayPosition(currentElements, t);

            // FIX: Move the visual child instead of the root object.
            // This keeps the LineRenderer perfectly anchored at Earth's center!
            if (visualBody != null)
            {
                visualBody.localPosition = displayPos;
            }
            else
            {
                transform.localPosition = displayPos; 
            }
        }

        private Vector3 GetDisplayPosition(OrbitalElements elements, double t)
        {
            Vector3 rawKm = KeplerianMath.GetPosition(elements, t);

            if (displayMode == OrbitDisplayMode.Macro)
            {
                return rawKm / ScaleConstants.KmPerMacroUnit;
            }
            else
            {
                Vector3 referenceKm = (relativeReference != null && relativeReference != this)
                    ? KeplerianMath.GetPosition(relativeReference.currentElements, t)
                    : Vector3.zero;

                Vector3 relativeMeters = (rawKm - referenceKm) * 1000f;
                return relativeMeters / ScaleConstants.EncounterSphereCompressionFactor;
            }
        }

        public void DrawOrbit()
        {
            double period = 2.0 * System.Math.PI * System.Math.Sqrt(System.Math.Pow(currentElements.semiMajorAxis, 3) / currentElements.mu);
            double timeStep = period / resolution;
            
            // FIX: Since the root object no longer moves, we don't need to offset the line renderer backward!
            Vector3 offset = (visualBody != null) ? Vector3.zero : transform.localPosition;

            for (int i = 0; i < resolution; i++)
            {
                double evalTime = currentElements.epoch + (i * timeStep);
                Vector3 displayPos = GetDisplayPosition(currentElements, evalTime);
                lineRenderer.SetPosition(i, displayPos - offset);
            }
        }
    }
}