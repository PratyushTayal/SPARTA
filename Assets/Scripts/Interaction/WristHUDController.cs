// NEW FILE — Text_Wrist_Data has been sitting at its default TextMeshPro
// placeholder text ("New Text") because nothing has ever written to it.
// This is the missing script.

using UnityEngine;
using TMPro;
using OrbitGuard.Managers;
using OrbitGuard.Core;

namespace OrbitGuard.UI
{
    public class WristHUDController : MonoBehaviour
    {
        public TextMeshProUGUI wristText;

        [Tooltip("Dry mass of the object being maneuvered, kg — used for the fuel cost readout.")]
        public float dryMassKg = 560f;

        private float lastDeltaVMagnitude = -1f;

        private void OnEnable()
        {
            if (RiskManager.Instance != null)
                RiskManager.Instance.OnPcUpdated += HandlePcUpdated;
        }

        private void OnDisable()
        {
            if (RiskManager.Instance != null)
                RiskManager.Instance.OnPcUpdated -= HandlePcUpdated;
        }

        private void HandlePcUpdated(float pc)
        {
            Redraw(pc);
        }

        private void Update()
        {
            // TCA countdown needs to refresh continuously even when Pc hasn't
            // changed, so this runs every frame — cheap, single string build.
            Redraw(null);
        }

        private void Redraw(float? pcOverride)
        {
            if (wristText == null) return;

            double simTime = TimeController.Instance != null ? TimeController.Instance.SimulationTime : 0.0;
            string pcLine = pcOverride.HasValue ? $"Pc: {pcOverride.Value:E2}" : "Pc: --";
            string missLine = RiskManager.Instance != null ? $"Miss: {RiskManager.Instance.CurrentLiveMissDistanceKm:F3} km" : "Miss: --";

            wristText.text = $"<b>{pcLine}</b>\n{missLine}\nSim T+: {simTime:F0}s";
        }
    }
}