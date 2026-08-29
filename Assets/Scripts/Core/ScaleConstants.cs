namespace OrbitGuard.Core
{
    /// <summary>
    /// SINGLE SOURCE OF TRUTH: All scale conversions, visual mapping, and planet dimensions.
    /// </summary>
    public static class ScaleConstants
    {
        // Real-World Planetary Dimensions
        public const float EarthRadiusKm = 6371.0f;
        public const float LeoAltitudeMinKm = 160.0f;
        public const float LeoAltitudeMaxKm = 2000.0f;

        // Visual Display Mappings (Macro Deck)
        public const float EarthVisualRadiusMeters = 5.0f; // 10-meter diameter map
        public const float KmPerMacroUnit = 1000.0f; 
        public const float MacroDisplayAltitudeBandMeters = 2.0f; // Orbit ring thickness
        
        // Visual Display Mappings (Encounter Sphere)
        // Real meters of separation compressed into 1 Unity meter, e.g. a 584m
        // real miss distance / 150 = a walkable ~3.9m gap in the Encounter Sphere.
        public const float EncounterSphereCompressionFactor = 150.0f; 
    }
}