// REPLACES your ConjunctionManager.cs. Fixes:
// - Item 1 (texts never populate): now actually calls VRCommandConsole.DisplayRawCdm
// - Item 8 (only Iridium/Cosmos text, no debris): was only ever initializing
//   ONE propagator pair; now initializes FOUR (macro sat, macro debris,
//   encounter sat, encounter debris) since you have two representations
//   of each object now
// - Pushes activeCdm into VectorGrabController and TimelineScrubberController
//   so their Pc math has real data instead of an all-zero struct
// - Pushes real covariance into both CovarianceBubbleController instances
//   so the uncertainty bubbles are sized from real CDM data, not left at
//   their default (invisible, scale-zero) state

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

                // Item 1 fix — this call never existed before.
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

                // Item 8 fix — all FOUR propagators get initialized now, not just two.
                if (macroSatellite != null) macroSatellite.Initialize(primaryElements);
                if (macroDebris != null) macroDebris.Initialize(secondaryElements);
                if (encounterSatellite != null) encounterSatellite.Initialize(primaryElements);
                if (encounterDebris != null) encounterDebris.Initialize(secondaryElements);

                // Push the real CDM into every script whose Pc math needs it —
                // without this, VectorGrabController/TimelineScrubberController
                // compute risk against an all-zero ConjunctionData forever.
                if (vectorGrabController != null) vectorGrabController.activeCdm = cdmData;
                if (timelineScrubber != null) timelineScrubber.activeCdm = cdmData;

                // Real covariance into the bubbles — without this they stay
                // at their default (0,0,0) scale and are invisible.
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
            }
            else
            {
                Debug.LogError("Failed to parse CDM: " + error);
            }
        }
    }
}