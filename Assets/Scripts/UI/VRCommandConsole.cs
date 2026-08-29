using UnityEngine;
using TMPro; 
using OrbitGuard.Managers;
using OrbitGuard.Data;
using OrbitGuard.Interaction;

namespace OrbitGuard.UI
{
    public class VRCommandConsole : MonoBehaviour
    {
        [Header("UI Panels")]
        [Tooltip("Drag your TextMeshPro text for the CDM here")]
        public TextMeshProUGUI cdmDisplayData;
        
        [Tooltip("Drag your TextMeshPro text for the Risk here")]
        public TextMeshProUGUI riskDisplayData;

        [Header("Physical Controls")]
        [Tooltip("Drag your Placeholder_Satellite's Thruster_Nozzle here")]
        public ThrusterModule satelliteThruster;

        private void OnEnable()
        {
            // Subscribe to the Risk Manager so the screen updates instantly
            if (RiskManager.Instance != null)
                RiskManager.Instance.OnPcUpdated += UpdateRiskUI;
        }

        private void OnDisable()
        {
            if (RiskManager.Instance != null)
                RiskManager.Instance.OnPcUpdated -= UpdateRiskUI;
        }

        public void DisplayRawCdm(ConjunctionData cdm)
        {
            if (cdmDisplayData != null)
            {
                cdmDisplayData.text = $"<b>REAL-TIME INGESTION</b>\n\n" +
                                      $"<b>Primary:</b> {cdm.object1.objectName}\n" +
                                      $"<b>Secondary:</b> {cdm.object2.objectName}\n" +
                                      $"<b>Miss Distance:</b> {cdm.missDistanceKm} km\n" +
                                      $"<b>Rel Velocity:</b> {cdm.relativeSpeedKmPerSec} km/s";
            }
        }

        private void UpdateRiskUI(float newPc)
        {
            if (riskDisplayData != null)
            {
                // Format as Scientific Notation just like real astrodynamics tools
                riskDisplayData.text = $"<b>CURRENT RISK (2D-Pc):</b>\n<color=red>{newPc:E3}</color>"; 
            }
        }

        /// <summary>
        /// Hook this directly to your VR Button's "Select Entered" event!
        /// </summary>
        public void ConfirmManeuver()
        {
            Debug.Log("VRCommandConsole: Maneuver Locked In! Executing Burn.");
            
            if (satelliteThruster != null)
            {
                satelliteThruster.FireThruster();
            }

            if (TelemetryStateManager.Instance != null)
            {
                TelemetryStateManager.Instance.ProposeManeuver(TelemetryStateManager.Instance.CounterfactualTelemetry);
            }
        }
    }
}