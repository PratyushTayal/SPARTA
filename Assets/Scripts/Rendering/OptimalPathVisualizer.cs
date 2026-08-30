// NEW FILE — replaces the abandoned ParetoFrontierVisualizer UI panel.
// Instead of a 2D graph, this draws the top 3 candidate maneuvers as
// actual ghost orbit lines around Earth, using the SAME real Keplerian
// math as everything else — the judge sees three possible futures for the
// satellite directly in 3D space, with the recommended one highlighted.
//
// Attach to an empty GameObject under [Macro_Space_Orbital_Deck].

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using OrbitGuard.Core;
using OrbitGuard.AI;

namespace OrbitGuard.Rendering
{
    public class OptimalPathVisualizer : MonoBehaviour
    {
        [Tooltip("Prefab: needs a LineRenderer, same setup as your orbit ring prefabs.")]
        public GameObject ghostLinePrefab;

        public Transform spawnParent; // same Earth-centered container as the debris cluster

        public int candidatesToShow = 3;
        public Color bestColor = new Color(0.66f, 0.33f, 0.97f);
        public Color runnerUpColor = new Color(0.5f, 0.7f, 1f, 0.4f);

        private List<GameObject> spawnedGhosts = new List<GameObject>();

        public void ShowFrontier(List<ManeuverCandidate> frontier, OrbitalElements baselineSatelliteElements)
        {
            ClearGhosts();
            if (frontier == null || frontier.Count == 0) return;

            // Rank by the same balanced-score approach as before: normalized
            // fuel + Pc + deviation, equal-weighted. Lowest score = "best."
            float minFuel = frontier.Min(c => c.fuelCostKg);
            float maxFuel = frontier.Max(c => c.fuelCostKg);
            float minPc = frontier.Min(c => c.resultingPc);
            float maxPc = frontier.Max(c => c.resultingPc);
            float minDev = frontier.Min(c => c.deviationPenalty);
            float maxDev = frontier.Max(c => c.deviationPenalty);

            var ranked = frontier
                .Select(c => new
                {
                    Candidate = c,
                    Score = SafeNorm(c.fuelCostKg, minFuel, maxFuel)
                          + SafeNorm(c.resultingPc, minPc, maxPc)
                          + SafeNorm(c.deviationPenalty, minDev, maxDev)
                })
                .OrderBy(x => x.Score)
                .Take(candidatesToShow)
                .ToList();

            for (int i = 0; i < ranked.Count; i++)
            {
                ManeuverCandidate candidate = ranked[i].Candidate;
                bool isBest = (i == 0);

                OrbitalElements ghostElements = ApplyDeltaVApproximation(baselineSatelliteElements, candidate.deltaV);

                GameObject ghost = Instantiate(ghostLinePrefab, spawnParent);
                OrbitPropagator prop = ghost.GetComponent<OrbitPropagator>();
                if (prop != null)
                {
                    prop.displayMode = OrbitDisplayMode.Macro;
                    prop.Initialize(ghostElements);
                }

                LineRenderer lr = ghost.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    Color c = isBest ? bestColor : runnerUpColor;
                    lr.startColor = c;
                    lr.endColor = c;
                    lr.startWidth = isBest ? 0.04f : 0.015f;
                    lr.endWidth = isBest ? 0.04f : 0.015f;
                }

                spawnedGhosts.Add(ghost);
            }

            Debug.Log($"OptimalPathVisualizer: Showing top {ranked.Count} candidates — best has fuel={ranked[0].Candidate.fuelCostKg:F3}kg, Pc={ranked[0].Candidate.resultingPc:E2}, deviation={ranked[0].Candidate.deviationPenalty:F2}.");
        }

        private float SafeNorm(float value, float min, float max)
        {
            float range = max - min;
            return range > 0.0001f ? (value - min) / range : 0.5f;
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

        private void ClearGhosts()
        {
            foreach (var g in spawnedGhosts) if (g != null) Destroy(g);
            spawnedGhosts.Clear();
        }
    }
}