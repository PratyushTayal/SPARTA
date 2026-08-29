using UnityEngine;
using System;

namespace OrbitGuard.Core
{
    public static class FuelCostCalculator
    {
        // Hydrazine monopropellant Isp (Specific Impulse)
        public const float TypicalHydrazineIsp = 220f; 
        
        // Standard Earth Gravity (m/s^2)
        public const float G0 = 9.80665f;

        /// <summary>
        /// Calculates the mass of fuel burned (in kg) for a given maneuver.
        /// Delta V must be in meters per second (m/s).
        /// </summary>
        public static float CalculatePropellantCost(float dryMassKg, float deltaV_MetersPerSecond)
        {
            // Tsiolkovsky Rocket Equation
            float exponent = -deltaV_MetersPerSecond / (TypicalHydrazineIsp * G0);
            float massCost = dryMassKg * (1f - (float)Math.Exp(exponent));
            
            return massCost;
        }
    }
}