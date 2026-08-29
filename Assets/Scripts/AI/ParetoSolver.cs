using UnityEngine;
using System.Collections.Generic;
using OrbitGuard.Core;
using OrbitGuard.Data;

namespace OrbitGuard.AI
{
    /// <summary>
    /// Holds the data for one potential "What-If" maneuver.
    /// </summary>
    public struct ManeuverCandidate
    {
        public Vector3 deltaV; // The burn vector in m/s
        public float fuelCostKg;
        public float resultingPc;
        public float deviationPenalty;
    }

    public class ParetoSolver : MonoBehaviour
    {
        public static ParetoSolver Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// Generates a grid of possible burns, evaluates them, and filters out the bad ones.
        /// </summary>
        public List<ManeuverCandidate> ComputeParetoFrontier(ConjunctionData cdmData, float dryMassKg)
        {
            List<ManeuverCandidate> allCandidates = new List<ManeuverCandidate>();
            List<ManeuverCandidate> paretoFrontier = new List<ManeuverCandidate>();

            // 1. Generate a small grid of test burns from -2 m/s to +2 m/s
            // (Keeping the grid small ensures the Quest 3S VR headset doesn't lag)
            float[] dvSamples = { -2f, -0.5f, 0.1f, 0.5f, 2f }; 

            foreach (float dx in dvSamples)
            {
                foreach (float dy in dvSamples)
                {
                    foreach (float dz in dvSamples)
                    {
                        Vector3 testBurn = new Vector3(dx, dy, dz);
                        
                        // Skip a zero-burn (doing nothing doesn't cost fuel, but doesn't fix the crash)
                        if (testBurn.magnitude == 0) continue;

                        // 2. Evaluate Objective 1: Fuel Cost
                        float fuel = FuelCostCalculator.CalculatePropellantCost(dryMassKg, testBurn.magnitude);

                        // 3. Evaluate Objective 2: Safety (Pc)
                        // Note: For hackathon performance, we use a proportional sensitivity model to preview 
                        // the new Pc rather than recalculating the full matrix inverse 125 times per frame.
                        float reductionFactor = 1f - Mathf.Clamp01(testBurn.magnitude / 2.5f);
                        float predictedPc = (float)cdmData.reportedCollisionProbability * reductionFactor;

                        // 4. Evaluate Objective 3: Deviation Penalty
                        // Penalize burns that are unnecessarily large and knock the satellite off its mission path
                        float deviation = testBurn.magnitude * 0.15f; 

                        allCandidates.Add(new ManeuverCandidate
                        {
                            deltaV = testBurn,
                            fuelCostKg = fuel,
                            resultingPc = predictedPc,
                            deviationPenalty = deviation
                        });
                    }
                }
            }

            // 5. The Pareto Filter (Non-Dominated Sorting)
            // Compare every candidate against every other candidate to see if it is strictly worse.
            for (int i = 0; i < allCandidates.Count; i++)
            {
                bool isDominated = false;
                for (int j = 0; j < allCandidates.Count; j++)
                {
                    if (i == j) continue;

                    // If Candidate J is better or equal in ALL THREE categories compared to Candidate I...
                    if (allCandidates[j].fuelCostKg <= allCandidates[i].fuelCostKg &&
                        allCandidates[j].resultingPc <= allCandidates[i].resultingPc &&
                        allCandidates[j].deviationPenalty <= allCandidates[i].deviationPenalty)
                    {
                        // ...and is strictly better in at least ONE category...
                        if (allCandidates[j].fuelCostKg < allCandidates[i].fuelCostKg ||
                            allCandidates[j].resultingPc < allCandidates[i].resultingPc ||
                            allCandidates[j].deviationPenalty < allCandidates[i].deviationPenalty)
                        {
                            // ...then Candidate I is garbage. Throw it away.
                            isDominated = true;
                            break;
                        }
                    }
                }

                // If no other burn was mathematically superior in all ways, it survives!
                if (!isDominated)
                {
                    paretoFrontier.Add(allCandidates[i]);
                }
            }

            Debug.Log($"ParetoSolver: Tested {allCandidates.Count} burns. Found {paretoFrontier.Count} optimal paths.");
            return paretoFrontier;
        }
    }
}