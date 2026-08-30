using UnityEngine;

public class SatelliteCollision : MonoBehaviour
{
    [SerializeField] private Transform brokenSatellite;
    [SerializeField] private float explosionForce = 500f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private GameObject explosionEffect;
    public bool broken=false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Debris") && !broken)
        {
            Transform brokenSat = Instantiate(brokenSatellite, transform.position, transform.rotation, this.transform);
            GameObject expEffect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(transform.GetChild(0).gameObject);
            Destroy(expEffect, 2f);
            foreach (Rigidbody rb in brokenSat.GetComponentsInChildren<Rigidbody>())
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0f, ForceMode.Impulse);
            }
            broken = true;
        }
    }
}
