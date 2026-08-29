namespace OrbitGuard.Core
{
    /// <summary>
    /// SINGLE SOURCE OF TRUTH: All real-world astrodynamics, masses, and engineering numbers.
    /// Do not hardcode these values anywhere else in the project.
    /// </summary>
    public static class PhysicalConstants
    {
        // Object Masses
        public const float Iridium33DryMassKg = 560f;
        public const float Cosmos2251DryMassKg = 900f;

        // Astrodynamics Fundamentals
        public const double EarthMu = 398600.4418; // km³/s²
        public const float StandardGravityG0 = 9.80665f; // m/s²

        // Spacecraft Engineering (Hydrazine Monopropellant)
        public const float TypicalHydrazineIsp = 220f; // Seconds
        
        // Maneuver Bounds (Pareto Grid Limits)
        public const float MinCamDeltaV_Mps = 0.05f;
        public const float MaxCamDeltaV_Mps = 2.0f;

        // Reference Geometries
        public const float DefaultCombinedHbrMeters = 20.0f;
        public const float TypicalDebrisFragmentRadiusMeters = 0.126f; // ~12.6 cm
    }
}