using UnityEngine;
using System;

namespace OrbitGuard.Core
{
    public static class KeplerianMath
    {
        public static Vector3 GetPosition(OrbitalElements elements, double currentTime)
        {
            // 1. Mean Motion & Anomaly
            double n = Math.Sqrt(elements.mu / Math.Pow(elements.semiMajorAxis, 3));
            double M = elements.meanAnomalyAtEpoch + n * (currentTime - elements.epoch);
            M = M % (2.0 * Math.PI);
            if (M < 0) M += 2.0 * Math.PI;

            // 2. Newton-Raphson solve for Eccentric Anomaly (E)
            double E = M;
            for (int i = 0; i < 5; i++)
            {
                double deltaE = (E - elements.eccentricity * Math.Sin(E) - M) / (1.0 - elements.eccentricity * Math.Cos(E));
                E -= deltaE;
            }

            // 3. True Anomaly (nu)
            double nu = 2.0 * Math.Atan2(
                Math.Sqrt(1.0 + elements.eccentricity) * Math.Sin(E / 2.0),
                Math.Sqrt(1.0 - elements.eccentricity) * Math.Cos(E / 2.0)
            );

            // 4. Position in Orbital Plane
            double r = elements.semiMajorAxis * (1.0 - elements.eccentricity * Math.Cos(E));
            double xOrb = r * Math.Cos(nu);
            double yOrb = r * Math.Sin(nu);

            // 5. 3D Rotation Matrices
            double cosO = Math.Cos(elements.raan);
            double sinO = Math.Sin(elements.raan);
            double cosw = Math.Cos(elements.argOfPeriapsis);
            double sinw = Math.Sin(elements.argOfPeriapsis);
            double cosi = Math.Cos(elements.inclination);
            double sini = Math.Sin(elements.inclination);

            double x = xOrb * (cosO * cosw - sinO * sinw * cosi) - yOrb * (cosO * sinw + sinO * cosw * cosi);
            double y = xOrb * (sinO * cosw + cosO * sinw * cosi) - yOrb * (sinO * sinw - cosO * cosw * cosi);
            double z = xOrb * (sinw * sini) + yOrb * (cosw * sini);

            // Convert ECI space (Z up) to Unity space (Y up)
            return new Vector3((float)x, (float)z, (float)y);
        }
    }
}