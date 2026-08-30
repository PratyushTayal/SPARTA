using UnityEngine;
using System.IO;
using OrbitGuard.Data;
using OrbitGuard.Core;
using OrbitGuard.Interaction;
using OrbitGuard.UI;
using OrbitGuard.Rendering;

namespace OrbitGuard.Managers
{
    public class ConjunctionManager : MonoBehaviour
    {
        [Header("Macro Deck Propagators")]
        public OrbitPropagator macroSatellite;
        public OrbitPropagator macroDebris;

        [Header("Encounter Sphere Propagators")]
        public OrbitPropagator encounterSatellite;
        public OrbitPropagator encounterDebris;

        [Header("UI")]
        public VRCommandConsole commandConsole;

        [Header("Interaction Scripts")]
        public VectorGrabController vectorGrabController;
        public TimelineScrubberController timelineScrubber;

        [Header("Covariance Bubbles")]
        public CovarianceBubbleController satelliteBubble;
        public CovarianceBubbleController debrisBubble;

        [Header("Debris Cluster")]
        public DebrisClusterManager debrisClusterManager;

        [Header("New Real-Math Modules")]
        public OptimalPathVisualizer optimalPathVisualizer;
        public CollisionOutcomeManager collisionOutcomeManager;

        void Start()
        {
            LoadDemoConjunction();
        }

        public void LoadDemoConjunction()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "SampleCDMs/DEMO-CDM-001.txt");

            if (!File.Exists(filePath))
            {
                Debug.LogError("CDM file not found at: " + filePath);
                return;
            }

            string rawText = File.ReadAllText(filePath);

            if (CdmParser.TryParseKvn(rawText, 0, out ConjunctionData cdmData, out string error))
            {
                // Anchor the time
                cdmData.tcaSeconds = 86400.0;
                
                // --- THE NEW FIX ---
                // Tell the TimeController exactly when the collision happens so it can slow down!
                if (TimeController.Instance != null)
                {
                    TimeController.Instance.criticalEventTimeSeconds = cdmData.tcaSeconds;
                }
                // -------------------

                Debug.Log($"SUCCESS: Loaded CDM for {cdmData.object1.objectName} vs {cdmData.object2.objectName}. TCA re-anchored to {cdmData.tcaSeconds}s.");

                if (commandConsole != null)
                    commandConsole.DisplayRawCdm(cdmData);

                if (RiskManager.Instance != null)
                    RiskManager.Instance.EvaluateCurrentRisk(cdmData);

                OrbitalElements primaryElements = CdmParser.ToOrbitalElements(cdmData.object1, cdmData.tcaSeconds);
                OrbitalElements secondaryElements = CdmParser.ToOrbitalElements(cdmData.object2, cdmData.tcaSeconds);

                if (OrbitGuard.AI.ParetoSolver.Instance != null)
                {
                    var frontier = OrbitGuard.AI.ParetoSolver.Instance.ComputeParetoFrontier(cdmData, (float)cdmData.object1.massKg);
                    if (optimalPathVisualizer != null)
                        optimalPathVisualizer.ShowFrontier(frontier, primaryElements);
                }

                if (TelemetryStateManager.Instance != null)
                    TelemetryStateManager.Instance.IngestNewTelemetry(primaryElements);

                if (macroSatellite != null) macroSatellite.Initialize(primaryElements);
                if (macroDebris != null) macroDebris.Initialize(secondaryElements);
                if (encounterSatellite != null) encounterSatellite.Initialize(primaryElements);
                if (encounterDebris != null) encounterDebris.Initialize(secondaryElements);

                if (timelineScrubber != null) timelineScrubber.activeCdm = cdmData;

                if (satelliteBubble != null)
                {
                    satelliteBubble.baseCovariance = cdmData.object1.covariance;
                    satelliteBubble.cdmEpochSeconds = cdmData.tcaSeconds;
                }
                if (debrisBubble != null)
                {
                    debrisBubble.baseCovariance = cdmData.object2.covariance;
                    debrisBubble.cdmEpochSeconds = cdmData.tcaSeconds;
                }

                if (debrisClusterManager != null)
                    debrisClusterManager.GenerateCluster(secondaryElements); 

                if (collisionOutcomeManager != null)
                    collisionOutcomeManager.ResetOutcome(cdmData.tcaSeconds);
            }
            else
            {
                Debug.LogError("Failed to parse CDM: " + error);
            }
        }
    }
}