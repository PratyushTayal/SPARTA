using UnityEngine;

public class CollisionDemo : MonoBehaviour
{
    [SerializeField] private Transform satellite;
    [SerializeField] private Transform debris;

    [SerializeField] private float satelliteSpeed = 5f;
    [SerializeField] private float debrisSpeed = 5f;

    private Rigidbody satelliteRb;
    private Rigidbody debrisRb;

    private void Start()
    {
        satelliteRb = satellite.GetComponent<Rigidbody>();
        debrisRb = debris.GetComponent<Rigidbody>();

        satelliteRb.isKinematic = false;
        debrisRb.isKinematic = false;

        satelliteRb.linearVelocity = satellite.forward * satelliteSpeed;
        debrisRb.linearVelocity = -debris.forward * debrisSpeed;
    }
}