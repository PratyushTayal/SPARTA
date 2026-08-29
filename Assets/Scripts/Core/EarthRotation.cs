// NEW FILE — item 5, "no earth rotation." Attach to Earth_Map_Scale.
using UnityEngine;

namespace OrbitGuard.Rendering
{
    public class EarthRotation : MonoBehaviour
    {
        [Tooltip("Degrees per second. Real Earth rotates once per ~24h (0.0042 deg/sec) — far too slow to notice in a short demo, so this defaults to a visually readable, deliberately non-realistic rate.")]
        public float degreesPerSecond = 4f;

        void Update()
        {
            transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.World);
        }
    }
}