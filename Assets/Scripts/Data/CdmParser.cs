using System;
using System.Collections.Generic;
using UnityEngine;
using OrbitGuard.Core;

namespace OrbitGuard.Data
{
    public static class CdmParser
    {
        public static bool TryParseKvn(string rawText, double timeBaseReferenceUnixSeconds, out ConjunctionData result, out string error)
        {
            result = default;
            error = null;

            if (string.IsNullOrWhiteSpace(rawText))
            {
                error = "CDM text was empty.";
                return false;
            }

            Dictionary<string, string> fields;
            try { fields = TokenizeKvn(rawText); }
            catch (Exception ex) { error = $"Failed to tokenize: {ex.Message}"; return false; }

            try
            {
                var cdm = new ConjunctionData
                {
                    conjunctionId = GetString(fields, "CONJUNCTION_ID", "UNKNOWN"),
                    creationDateUtc = GetString(fields, "CREATION_DATE", ""),
                    missDistanceKm = GetDouble(fields, "MISS_DISTANCE", 0.0),
                    relativeSpeedKmPerSec = GetDouble(fields, "RELATIVE_SPEED", 0.0)
                };

                cdm.tcaSeconds = ParseUtcToProjectSeconds(GetString(fields, "TCA", null), timeBaseReferenceUnixSeconds);
                
                if (fields.TryGetValue("COLLISION_PROBABILITY", out string pcRaw) && double.TryParse(pcRaw, out double pcValue))
                {
                    cdm.reportedCollisionProbability = pcValue;
                    cdm.hasReportedCollisionProbability = true;
                }
                else { cdm.hasReportedCollisionProbability = false; }

                cdm.object1 = ParseObjectBlock(fields, "OBJECT1_", cdm.tcaSeconds);
                cdm.object2 = ParseObjectBlock(fields, "OBJECT2_", cdm.tcaSeconds);

                result = cdm;
                return true;
            }
            catch (Exception ex) { error = $"Failed to build data: {ex.Message}"; return false; }
        }

        private static Dictionary<string, string> TokenizeKvn(string rawText)
        {
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string currentObjectPrefix = ""; 

            string[] lines = rawText.Replace("\r\n", "\n").Split('\n');
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("COMMENT", StringComparison.OrdinalIgnoreCase)) continue;

                int equalsIndex = line.IndexOf('=');
                if (equalsIndex < 0) continue; 

                string key = line.Substring(0, equalsIndex).Trim();
                string value = line.Substring(equalsIndex + 1).Trim();

                int bracketIndex = value.IndexOf('[');
                if (bracketIndex >= 0) value = value.Substring(0, bracketIndex).Trim();

                if (string.Equals(key, "OBJECT", StringComparison.OrdinalIgnoreCase))
                {
                    if (value.Equals("OBJECT1", StringComparison.OrdinalIgnoreCase)) currentObjectPrefix = "OBJECT1_";
                    else if (value.Equals("OBJECT2", StringComparison.OrdinalIgnoreCase)) currentObjectPrefix = "OBJECT2_";
                    continue; 
                }

                string storedKey = key.StartsWith("OBJECT1_", StringComparison.OrdinalIgnoreCase) || key.StartsWith("OBJECT2_", StringComparison.OrdinalIgnoreCase)
                    ? key : currentObjectPrefix + key;   

                fields[storedKey] = value;
            }
            return fields;
        }

        private static CdmObjectData ParseObjectBlock(Dictionary<string, string> fields, string prefix, double tcaSeconds)
        {
            var obj = new CdmObjectData
            {
                objectDesignator = GetString(fields, prefix + "OBJECT_DESIGNATOR", "UNKNOWN"),
                objectName = GetString(fields, prefix + "OBJECT_NAME", "UNKNOWN"),
                objectType = GetString(fields, prefix + "OBJECT_TYPE", "UNKNOWN"),
                x = GetDouble(fields, prefix + "X", 0.0), y = GetDouble(fields, prefix + "Y", 0.0), z = GetDouble(fields, prefix + "Z", 0.0),
                xDot = GetDouble(fields, prefix + "X_DOT", 0.0), yDot = GetDouble(fields, prefix + "Y_DOT", 0.0), zDot = GetDouble(fields, prefix + "Z_DOT", 0.0),
                hardBodyRadiusMeters = GetDouble(fields, prefix + "HBR", 0.0), massKg = GetDouble(fields, prefix + "MASS", 0.0)
            };

            obj.covariance = new CovarianceMatrix
            {
                crR = GetDouble(fields, prefix + "CR_R", double.NaN), ctT = GetDouble(fields, prefix + "CT_T", double.NaN), cnN = GetDouble(fields, prefix + "CN_N", double.NaN),
                ctR = GetDouble(fields, prefix + "CT_R", 0.0), cnR = GetDouble(fields, prefix + "CN_R", 0.0), cnT = GetDouble(fields, prefix + "CN_T", 0.0)
            };

            if (double.IsNaN(obj.covariance.crR) || double.IsNaN(obj.covariance.ctT) || double.IsNaN(obj.covariance.cnN))
                obj.covariance = CovarianceMatrix.DefaultConservativeEstimate();

            return obj;
        }

        public static OrbitalElements ToOrbitalElements(CdmObjectData obj, double epochSeconds, double mu = 398600.4418)
        {
            return StateVectorConverter.ToKeplerianElements(obj.x, obj.y, obj.z, obj.xDot, obj.yDot, obj.zDot, epochSeconds, mu);
        }

        private static string GetString(Dictionary<string, string> fields, string key, string fallback) => fields.TryGetValue(key, out string value) ? value : fallback;
        private static double GetDouble(Dictionary<string, string> fields, string key, double fallback) => fields.TryGetValue(key, out string value) && double.TryParse(value, out double result) ? result : fallback;

        private static double ParseUtcToProjectSeconds(string isoUtcString, double timeBaseReferenceUnixSeconds)
        {
            if (string.IsNullOrEmpty(isoUtcString)) return 0.0;
            if (DateTime.TryParse(isoUtcString, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime parsed))
            {
                double unixSeconds = (parsed - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                return unixSeconds - timeBaseReferenceUnixSeconds;
            }
            return 0.0;
        }
    }
}