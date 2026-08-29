using System;
using OrbitGuard.Data;

namespace OrbitGuard.Core
{
    public static class StateVectorConverter
    {
        private const double TwoPi = 2.0 * Math.PI;
        private const double SmallTolerance = 1e-10;

        private struct Vec3d
        {
            public double x, y, z;
            public Vec3d(double x, double y, double z) { this.x = x; this.y = y; this.z = z; }
            public static Vec3d operator +(Vec3d a, Vec3d b) => new Vec3d(a.x + b.x, a.y + b.y, a.z + b.z);
            public static Vec3d operator -(Vec3d a, Vec3d b) => new Vec3d(a.x - b.x, a.y - b.y, a.z - b.z);
            public static Vec3d operator *(Vec3d a, double s) => new Vec3d(a.x * s, a.y * s, a.z * s);
            public double Dot(Vec3d o) => x * o.x + y * o.y + z * o.z;
            public Vec3d Cross(Vec3d o) => new Vec3d(y * o.z - z * o.y, z * o.x - x * o.z, x * o.y - y * o.x);
            public double Magnitude => Math.Sqrt(Dot(this));
        }

        public static OrbitalElements ToKeplerianElements(double x, double y, double z, double xDot, double yDot, double zDot, double epochSeconds, double mu = 398600.4418)
        {
            var r = new Vec3d(x, y, z);
            var v = new Vec3d(xDot, yDot, zDot);
            double rMag = r.Magnitude;
            double vMag = v.Magnitude;

            Vec3d h = r.Cross(v);
            double hMag = h.Magnitude;
            Vec3d n = new Vec3d(0.0, 0.0, 1.0).Cross(h);
            double nMag = n.Magnitude;

            double rDotV = r.Dot(v);
            Vec3d eVec = (r * (vMag * vMag - mu / rMag) - v * rDotV) * (1.0 / mu);
            double eccentricity = eVec.Magnitude;

            double specificEnergy = (vMag * vMag) / 2.0 - mu / rMag;
            double semiMajorAxis = Math.Abs(specificEnergy) > SmallTolerance ? -mu / (2.0 * specificEnergy) : double.PositiveInfinity;
            double inclination = Math.Acos(Clamp(h.z / hMag, -1.0, 1.0));

            double raan = 0.0;
            if (nMag > SmallTolerance)
            {
                raan = Math.Acos(Clamp(n.x / nMag, -1.0, 1.0));
                if (n.y < 0.0) raan = TwoPi - raan;
            }

            double argOfPeriapsis = 0.0;
            if (nMag > SmallTolerance && eccentricity > SmallTolerance)
            {
                double cosArgP = n.Dot(eVec) / (nMag * eccentricity);
                argOfPeriapsis = Math.Acos(Clamp(cosArgP, -1.0, 1.0));
                if (eVec.z < 0.0) argOfPeriapsis = TwoPi - argOfPeriapsis;
            }

            double trueAnomaly;
            if (eccentricity > SmallTolerance)
            {
                double cosNu = eVec.Dot(r) / (eccentricity * rMag);
                trueAnomaly = Math.Acos(Clamp(cosNu, -1.0, 1.0));
                if (rDotV < 0.0) trueAnomaly = TwoPi - trueAnomaly;
            }
            else
            {
                if (nMag > SmallTolerance)
                {
                    double cosU = n.Dot(r) / (nMag * rMag);
                    trueAnomaly = Math.Acos(Clamp(cosU, -1.0, 1.0));
                    if (r.z < 0.0) trueAnomaly = TwoPi - trueAnomaly;
                }
                else
                {
                    trueAnomaly = Math.Acos(Clamp(r.x / rMag, -1.0, 1.0));
                    if (r.y < 0.0) trueAnomaly = TwoPi - trueAnomaly;
                }
            }

            double eccentricAnomaly = 2.0 * Math.Atan2(Math.Sqrt(1.0 - eccentricity) * Math.Sin(trueAnomaly / 2.0), Math.Sqrt(1.0 + eccentricity) * Math.Cos(trueAnomaly / 2.0));
            double meanAnomaly = NormalizePositive(eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly));

            return new OrbitalElements
            {
                semiMajorAxis = semiMajorAxis, eccentricity = eccentricity, inclination = inclination,
                raan = NormalizePositive(raan), argOfPeriapsis = NormalizePositive(argOfPeriapsis),
                meanAnomalyAtEpoch = meanAnomaly, epoch = epochSeconds, mu = mu
            };
        }

        private static double Clamp(double value, double min, double max) => Math.Max(min, Math.Min(max, value));
        private static double NormalizePositive(double angleRadians)
        {
            double result = angleRadians % TwoPi;
            if (result < 0.0) result += TwoPi;
            return result;
        }
    }
}