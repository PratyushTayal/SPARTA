using UnityEngine;

namespace OrbitGuard.Rendering
{
    public class EarthRotation : MonoBehaviour
    {
        public float degreesPerSecond = 4f;

        void Update()
        {
            transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.World);
        }
    }
}