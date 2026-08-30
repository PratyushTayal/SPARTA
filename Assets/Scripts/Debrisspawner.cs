using UnityEngine;

public class DebrisSpawner : MonoBehaviour
{
    [System.Serializable]
    public class DebrisType
    {
        public GameObject prefab;
        public int count = 5;

        public Vector2 scaleRange = new Vector2(0.02f, 0.06f);
    }

    [Header("References")]
    public Transform earth;
    public DebrisType[] debrisTypes;

    [Header("Orbit Ranges (as a multiple of Earth's radius)")]
    public Vector2 orbitRadiusMultiplierRange = new Vector2(1.5f, 4f);
    public Vector2 orbitSpeedRange = new Vector2(5f, 40f); // degrees/sec

    void Start()
    {
        if (earth == null)
        {
            Debug.LogWarning("DebrisSpawner: no earth assigned.");
            return;
        }

    
        float earthScale = earth.localScale.x;
        float earthRadius = earthScale * 0.5f;

        foreach (var type in debrisTypes)
        {
            if (type.prefab == null) continue;

            for (int i = 0; i < type.count; i++)
            {
                Vector3 dir = Random.onUnitSphere;
                float radiusMultiplier = Random.Range(orbitRadiusMultiplierRange.x, orbitRadiusMultiplierRange.y);
                float orbitRadius = earthRadius * radiusMultiplier;

                Vector3 spawnPos = earth.position + dir * orbitRadius;
                GameObject instance = Instantiate(type.prefab, spawnPos, Random.rotation, transform);

                float scaleMultiplier = Random.Range(type.scaleRange.x, type.scaleRange.y);
                instance.transform.localScale = Vector3.one * earthScale * scaleMultiplier;

                OrbitAroundEarth orbit = instance.GetComponent<OrbitAroundEarth>();
                if (orbit == null) orbit = instance.AddComponent<OrbitAroundEarth>();

                orbit.earth = earth;
                orbit.orbitRadius = orbitRadius;
                orbit.orbitSpeed = Random.Range(orbitSpeedRange.x, orbitSpeedRange.y) * (Random.value > 0.5f ? 1f : -1f);
                orbit.orbitAxis = Random.onUnitSphere;
                orbit.selfSpinSpeed = Random.Range(10f, 90f);
                orbit.selfSpinAxis = Random.onUnitSphere;
            }
        }
    }
}