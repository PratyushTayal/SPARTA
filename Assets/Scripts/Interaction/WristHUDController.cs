using UnityEngine;
using TMPro;
using OrbitGuard.Managers;
using OrbitGuard.Core;

namespace OrbitGuard.UI
{
    public class WristHUDController : MonoBehaviour
    {
        public TextMeshProUGUI wristText;
        public float dryMassKg = 560f;

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