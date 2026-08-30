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

        [Header("Interaction Scripts Needing the Active CDM")]
        public VectorGrabController vectorGrabController;
        public TimelineScrubberController timelineScrubber;

        [Header("Covariance Bubbles")]
        public CovarianceBubbleController satelliteBubble;
        public CovarianceBubbleController debrisBubble;

        [Header("Debris Cluster")]
        public DebrisClusterManager debrisClusterManager;

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
                Debug.Log($"SUCCESS: Loaded CDM for {cdmData.object1.objectName} vs {cdmData.object2.objectName}");

                if (commandConsole != null)
                    commandConsole.DisplayRawCdm(cdmData);

                if (RiskManager.Instance != null)
                    RiskManager.Instance.EvaluateCurrentRisk(cdmData);

                if (OrbitGuard.AI.ParetoSolver.Instance != null)
                    OrbitGuard.AI.ParetoSolver.Instance.ComputeParetoFrontier(cdmData, (float)cdmData.object1.massKg);

                OrbitalElements primaryElements = CdmParser.ToOrbitalElements(cdmData.object1, cdmData.tcaSeconds);
                OrbitalElements secondaryElements = CdmParser.ToOrbitalElements(cdmData.object2, cdmData.tcaSeconds);

                if (TelemetryStateManager.Instance != null)
                    TelemetryStateManager.Instance.IngestNewTelemetry(primaryElements);

                if (macroSatellite != null) macroSatellite.Initialize(primaryElements);
                if (macroDebris != null) macroDebris.Initialize(secondaryElements);
                if (encounterSatellite != null) encounterSatellite.Initialize(primaryElements);
                if (encounterDebris != null) encounterDebris.Initialize(secondaryElements);

                if (vectorGrabController != null) vectorGrabController.activeCdm = cdmData;
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
                    // THE FIX IS HERE: No arguments inside GenerateCluster()
                    debrisClusterManager.GenerateCluster(); 
            }
            else
            {
                Debug.LogError("Failed to parse CDM: " + error);
            }
        }
    }
}