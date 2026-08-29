// REPLACES your RiskManager.cs. Keeps your existing math exactly as-is —
// only adds RecomputeFromLivePositions, which VectorGrabController and
// TimelineScrubberController call so the Pc number actually changes
// instead of being frozen at the CDM's originally-reported value forever.

using UnityEngine;
using System;
using OrbitGuard.Data;
using OrbitGuard.Core;

namespace OrbitGuard.Managers
{
    public struct Matrix2x2
    {
        public double cxx, cxy, cyx, cyy;
        public double Determinant() => (cxx * cyy) - (cxy * cyx);
    }

    public class RiskManager : MonoBehaviour
    {
        public static RiskManager Instance { get; private set; }

        public Action<float> OnPcUpdated;
        public Action<ConjunctionData> OnSecondaryRiskDetected;

        public double CurrentLiveMissDistanceKm { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public Matrix2x2 ProjectOntoEncounterPlane(CovarianceMatrix c1, CovarianceMatrix c2, Vector3 relativeVelocity)
        {
            return new Matrix2x2
            {
                cxx = c1.ctT + c2.ctT,
                cxy = c1.cnT + c2.cnT,
                cyx = c1.cnT + c2.cnT,
                cyy = c1.cnN + c2.cnN
            };
        }

        public float ComputePc(Vector2 missVectorInPlane, Matrix2x2 combinedCovariance, float combinedHbr)
        {
            double detC = combinedCovariance.Determinant();
            if (detC <= 0) return 0f;

            double invCxx = combinedCovariance.cyy / detC;
            double invCyy = combinedCovariance.cxx / detC;
            double invCxy = -combinedCovariance.cxy / detC;

            double dx = missVectorInPlane.x;
            double dy = missVectorInPlane.y;

            double exponentTerm = (dx * invCxx * dx) + (dx * invCxy * dy) + (dy * invCxy * dx) + (dy * invCyy * dy);

            double hbrSq = combinedHbr * combinedHbr;
            double preFactor = hbrSq / (2.0 * Math.Sqrt(detC));

            return (float)(preFactor * Math.Exp(-0.5 * exponentTerm));
        }

        /// <summary>Baseline call at scenario load — unchanged.</summary>
        public void EvaluateCurrentRisk(ConjunctionData cdmData)
        {
            float hbrKm = (float)cdmData.CombinedHardBodyRadiusMeters() / 1000f;
            Vector3 relVel = new Vector3(0, 0, (float)cdmData.relativeSpeedKmPerSec);

            Matrix2x2 encounterCovariance = ProjectOntoEncounterPlane(cdmData.object1.covariance, cdmData.object2.covariance, relVel);
            Vector2 missVector = new Vector2((float)cdmData.missDistanceKm, 0f);

            float calculatedPc = ComputePc(missVector, encounterCovariance, hbrKm);
            CurrentLiveMissDistanceKm = cdmData.missDistanceKm;

            Debug.Log($"RiskManager: Baseline Locally Computed Pc = {calculatedPc}");
            if (cdmData.hasReportedCollisionProbability)
                Debug.Log($"RiskManager: NASA Reported Pc = {cdmData.reportedCollisionProbability}");

            OnPcUpdated?.Invoke(calculatedPc);
        }

        /// <summary>
        /// THE FIX: call this continuously (VectorGrabController while
        /// held, TimelineScrubberController while scrubbing) so Pc is
        /// recomputed from where the objects ACTUALLY are, not the CDM's
        /// static original number.
        /// </summary>
        public void RecomputeFromLivePositions(ConjunctionData cdmData, OrbitalElements primaryElementsNow, OrbitalElements secondaryElementsNow, double evaluationTimeSeconds)
        {
            Vector3 posPrimaryKm = KeplerianMath.GetPosition(primaryElementsNow, evaluationTimeSeconds);
            Vector3 posSecondaryKm = KeplerianMath.GetPosition(secondaryElementsNow, evaluationTimeSeconds);
            Vector3 velPrimary = ApproximateVelocity(primaryElementsNow, evaluationTimeSeconds);
            Vector3 velSecondary = ApproximateVelocity(secondaryElementsNow, evaluationTimeSeconds);

            Vector3 relativePositionKm = posSecondaryKm - posPrimaryKm;
            Vector3 relativeVelocity = velSecondary - velPrimary;

            double liveMissDistanceKm = relativePositionKm.magnitude;
            CurrentLiveMissDistanceKm = liveMissDistanceKm;

            float hbrKm = (float)cdmData.CombinedHardBodyRadiusMeters() / 1000f;
            Matrix2x2 encounterCovariance = ProjectOntoEncounterPlane(cdmData.object1.covariance, cdmData.object2.covariance, relativeVelocity);
            Vector2 missVector = new Vector2((float)liveMissDistanceKm, 0f);

            float calculatedPc = ComputePc(missVector, encounterCovariance, hbrKm);
            OnPcUpdated?.Invoke(calculatedPc);
        }

        private Vector3 ApproximateVelocity(OrbitalElements elements, double t, double dt = 0.5)
        {
            Vector3 before = KeplerianMath.GetPosition(elements, t - dt);
            Vector3 after = KeplerianMath.GetPosition(elements, t + dt);
            return (after - before) / (float)(2.0 * dt);
        }
    }
}