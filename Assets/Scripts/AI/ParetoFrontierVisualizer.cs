using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using OrbitGuard.AI;

namespace OrbitGuard.AI
{
    public class ParetoFrontierVisualizer : MonoBehaviour
    {
        [Header("Markers")]
        [Tooltip("Small sphere prefab representing one candidate maneuver.")]
        public GameObject markerPrefab;

        [Tooltip("Parent transform for spawned markers — should be a flat panel facing the user.")]
        public Transform panelRoot;

        [Header("Panel Axis Scaling")]
        public float panelWidth = 0.3f;
        public float panelHeight = 0.2f;

        [Header("Colors")]
        public Color normalCandidateColor = new Color(0.6f, 0.75f, 1f);
        public Color bestCandidateColor = new Color(0.66f, 0.33f, 0.97f);

        private List<GameObject> spawnedMarkers = new List<GameObject>();
        private List<ManeuverCandidate> currentFrontier = new List<ManeuverCandidate>();

        public event System.Action<ManeuverCandidate> OnCandidateSelected;

        public void DisplayFrontier(List<ManeuverCandidate> frontier)
        {
            ClearMarkers();
            currentFrontier = frontier;

            if (frontier == null || frontier.Count == 0) return;

            float minFuel = frontier.Min(c => c.fuelCostKg);
            float maxFuel = frontier.Max(c => c.fuelCostKg);
            float minPc = frontier.Min(c => c.resultingPc);
            float maxPc = frontier.Max(c => c.resultingPc);

            ManeuverCandidate best = FindMostBalancedCandidate(frontier, minFuel, maxFuel, minPc, maxPc);

            foreach (var candidate in frontier)
            {
                float normalizedFuel = SafeNormalize(candidate.fuelCostKg, minFuel, maxFuel);
                float normalizedPc = SafeNormalize(candidate.resultingPc, minPc, maxPc);

                Vector3 localPos = new Vector3(
                    (normalizedFuel - 0.5f) * panelWidth,
                    (normalizedPc - 0.5f) * panelHeight,
                    0f);

                GameObject marker = Instantiate(markerPrefab, panelRoot);
                marker.transform.localPosition = localPos;

                bool isBest = candidate.Equals(best);
                marker.transform.localScale = Vector3.one * (isBest ? 0.025f : 0.015f);

                Renderer rend = marker.GetComponentInChildren<Renderer>();
                if (rend != null)
                    rend.material.color = isBest ? bestCandidateColor : normalCandidateColor;

                var selectable = marker.AddComponent<ParetoMarkerSelectable>();
                selectable.Initialize(candidate, this);

                spawnedMarkers.Add(marker);
            }
        }

        private ManeuverCandidate FindMostBalancedCandidate(List<ManeuverCandidate> frontier, float minFuel, float maxFuel, float minPc, float maxPc)
        {
            float minDev = frontier.Min(c => c.deviationPenalty);
            float maxDev = frontier.Max(c => c.deviationPenalty);

            ManeuverCandidate best = frontier[0];
            float bestScore = float.MaxValue;

            foreach (var c in frontier)
            {
                float score = SafeNormalize(c.fuelCostKg, minFuel, maxFuel)
                            + SafeNormalize(c.resultingPc, minPc, maxPc)
                            + SafeNormalize(c.deviationPenalty, minDev, maxDev);

                if (score < bestScore)
                {
                    bestScore = score;
                    best = c;
                }
            }

            return best;
        }

        private float SafeNormalize(float value, float min, float max)
        {
            float range = max - min;
            return range > 0.0001f ? (value - min) / range : 0.5f;
        }

        public void HandleMarkerSelected(ManeuverCandidate candidate)
        {
            OnCandidateSelected?.Invoke(candidate);
        }

        private void ClearMarkers()
        {
            foreach (var m in spawnedMarkers)
                if (m != null) Destroy(m);
            spawnedMarkers.Clear();
        }
    }

    public class ParetoMarkerSelectable : MonoBehaviour
    {
        private ManeuverCandidate candidate;
        private ParetoFrontierVisualizer owner;

        public void Initialize(ManeuverCandidate candidate, ParetoFrontierVisualizer owner)
        {
            this.candidate = candidate;
            this.owner = owner;
        }

        public void NotifySelected()
        {
            owner?.HandleMarkerSelected(candidate);
        }
    }
}