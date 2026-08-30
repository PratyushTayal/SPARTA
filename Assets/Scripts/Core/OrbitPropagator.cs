// REPLACES your OrbitPropagator.cs. This is the #1 fix — nearly every
// symptom you listed traces back to this file still being the flat
// "divide by 1000" version with no display-mode split and no code that
// ever moves the object's own Transform.

using UnityEngine;

namespace OrbitGuard.Core
{
    public enum OrbitDisplayMode
    {
        Macro,              // absolute position ÷ ScaleConstants.KmPerMacroUnit — for the map-view deck
        EncounterRelative   // position RELATIVE to relativeReference, compressed — for the walkable Encounter Sphere
    }

    [RequireComponent(typeof(LineRenderer))]
    public class OrbitPropagator : MonoBehaviour
    {
        public OrbitalElements currentElements;

        [Header("Display Mode")]
        public OrbitDisplayMode displayMode = OrbitDisplayMode.Macro;

        [Tooltip("ONLY used in EncounterRelative mode. Leave EMPTY for the primary satellite itself (it always renders at local origin 0,0,0). Set to the primary satellite's OrbitPropagator for debris/fragments.")]
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

            // FIX 1: Add the simulation elapsed time to the initial epoch. 
            // This ensures the math continuously calculates new positions as time ticks forward.
            double t = TimeController.Instance != null ? (currentElements.epoch + TimeController.Instance.SimulationTime) : currentElements.epoch;

            // Update the object's 3D position every frame based on the new time
            transform.localPosition = GetDisplayPosition(currentElements, t);

            // FIX 2: DrawOrbit() has been REMOVED from the Update loop!
            // It is already drawn once in Initialize(). Calling it here caused massive VR lag.
        }

        /// <summary>
        /// THE FIX for clustering AND "orbits appear around the camera not
        /// Earth": Macro mode divides by a fixed scale (correct for the map
        /// view). EncounterRelative mode subtracts the reference object's
        /// position at the SAME time — so what renders is the real relative
        /// geometry between two objects a few km apart, not their near-
        /// identical multi-thousand-km absolute distance from Earth's center.
        /// </summary>
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
            Vector3 selfPos = transform.localPosition; // LineRenderer is local-space; anchor the path relative to where the object itself currently sits

            for (int i = 0; i < resolution; i++)
            {
                double evalTime = currentElements.epoch + (i * timeStep);
                Vector3 displayPos = GetDisplayPosition(currentElements, evalTime);
                lineRenderer.SetPosition(i, displayPos - selfPos);
            }
        }
    }
}