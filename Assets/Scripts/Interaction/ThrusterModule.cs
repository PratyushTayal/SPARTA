using UnityEngine;

namespace OrbitGuard.Interaction
{
    public class ThrusterModule : MonoBehaviour
    {
        [Tooltip("Drag the Particle System attached to this thruster here")]
        public ParticleSystem thrusterPlume;
        
        [Tooltip("Drag an AudioSource here (optional for now)")]
        public AudioSource thrusterAudio;

        public void FireThruster()
        {
            if (thrusterPlume != null) 
            {
                thrusterPlume.Play();
            }
            
            if (thrusterAudio != null) 
            {
                thrusterAudio.Play();
            }
            
            Debug.Log("ThrusterModule: Burn committed! Thrusters firing.");
        }
    }
}