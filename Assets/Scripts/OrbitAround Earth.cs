using UnityEngine;

public class OrbitAroundEarth : MonoBehaviour
{
    [Header("Orbit Target")]
    public Transform earth;

    [Header("Orbit Settings")]
    public float orbitRadius = 10f;      
    public float orbitSpeed = 20f;       
    public Vector3 orbitAxis = Vector3.up; 

    [Header("Self Rotation (tumble, optional)")]
    public float selfSpinSpeed = 50f;
    public Vector3 selfSpinAxis = Vector3.one;

    void Start()
    {
        if (earth == null)
        {
            Debug.LogWarning("OrbitAroundEarth: no earth assigned on " + name);
            return;
        }

        Vector3 dir = (transform.position - earth.position).normalized;
        if (dir == Vector3.zero) dir = Random.onUnitSphere;
        transform.position = earth.position + dir * orbitRadius;
    }

    void Update()
    {
        if (earth == null) return;

        transform.RotateAround(earth.position, orbitAxis, orbitSpeed * Time.deltaTime);

        if (selfSpinSpeed != 0f)
            transform.Rotate(selfSpinAxis * selfSpinSpeed * Time.deltaTime, Space.Self);
    }
}