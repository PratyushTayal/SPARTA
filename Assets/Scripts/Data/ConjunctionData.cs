using System;
using UnityEngine;
using OrbitGuard.Core; // NEW: We need this to access PhysicalConstants

namespace OrbitGuard.Data
{
    [Serializable]
    public struct CovarianceMatrix
    {
        public double crR; 
        public double ctT; 
        public double cnN; 
        public double ctR; 
        public double cnR; 
        public double cnT; 

        public double[] ToMatrix3x3()
        {
            return new double[] { crR, ctR, cnR, ctR, ctT, cnT, cnR, cnT, cnN };
        }

        public static CovarianceMatrix DefaultConservativeEstimate()
        {
            return new CovarianceMatrix { crR = 1.0, ctT = 25.0, cnN = 1.0, ctR = 0.0, cnR = 0.0, cnT = 0.0 };
        }
    }

    [Serializable]
    public struct CdmObjectData
    {
        public string objectDesignator;   
        public string objectName;         
        public string objectType;         

        public double x, y, z;
        public double xDot, yDot, zDot;

        public CovarianceMatrix covariance;
        public double hardBodyRadiusMeters;
        public double massKg;
    }

    [Serializable]
    public struct ConjunctionData
    {
        public string conjunctionId;
        public string creationDateUtc;
        public double tcaSeconds;
        public double missDistanceKm;
        public double relativeSpeedKmPerSec;
        public double reportedCollisionProbability;
        public bool hasReportedCollisionProbability;

        public CdmObjectData object1; 
        public CdmObjectData object2; 

        public double CombinedHardBodyRadiusMeters()
        {
            double r1 = object1.hardBodyRadiusMeters;
            double r2 = object2.hardBodyRadiusMeters;
            
            // UPGRADE: We now pull this directly from the Single Source of Truth!
            if (r1 <= 0.0 && r2 <= 0.0) 
            {
                return PhysicalConstants.DefaultCombinedHbrMeters; 
            }
            
            return r1 + r2;
        }
    }
}