using System;

namespace OrbitGuard.Core
{
    [Serializable]
    public struct OrbitalElements
    {
        public double semiMajorAxis;
        public double eccentricity;
        public double inclination; // in radians
        public double raan; // in radians
        public double argOfPeriapsis; // in radians
        public double meanAnomalyAtEpoch; // in radians
        public double epoch; // in seconds
        public double mu;
    }
}