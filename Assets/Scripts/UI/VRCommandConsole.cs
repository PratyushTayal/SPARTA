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
        public TextMeshProUGUI cdmDisplayData;
        public TextMeshProUGUI riskDisplayData;

        [Header("Physical Controls")]
        public ThrusterModule satelliteThruster;

        private void Start()
        {
            if (RiskManager.Instance != null)
                RiskManager.Instance.OnPcUpdated += UpdateRiskUI;
            else
                Debug.LogWarning("VRCommandConsole: RiskManager.Instance was still null in Start() — check script execution order or that a RiskManager exists in the scene.");
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
                riskDisplayData.text = $"<b>CURRENT RISK (2D-Pc):</b>\n<color=red>{newPc:E3}</color>";
            }
        }

        public void ConfirmManeuver()
        {
            Debug.Log("VRCommandConsole: Maneuver Locked In! Executing Burn.");

            if (satelliteThruster != null)
                satelliteThruster.FireThruster();

            if (TelemetryStateManager.Instance != null)
                TelemetryStateManager.Instance.ProposeManeuver(TelemetryStateManager.Instance.CounterfactualTelemetry);
        }
    }
}