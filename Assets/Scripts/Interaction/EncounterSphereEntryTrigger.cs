using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

namespace OrbitGuard.Interaction
{
    public class EncounterSphereEntryTrigger : MonoBehaviour
    {
        public ZeroGFlightLocomotion zeroGLocomotion;

        [Tooltip("Your ground-based locomotion provider COMPONENT (e.g. ContinuousMoveProvider), not its GameObject.")]
        public LocomotionProvider groundLocomotionProvider;

        public CharacterController characterController;
        public Transform xrOriginRoot;
        public Transform encounterEntryPoint;
        public Transform deckEntryPoint;

        public float debounceSeconds = 0.5f;

        private bool isInsideEncounterSphere = false;
        private float lastToggleTime = -999f;

        public void Toggle()
        {
            if (Time.time - lastToggleTime < debounceSeconds)
            {
                Debug.LogWarning("EncounterSphereEntryTrigger: Toggle() called twice too quickly — ignoring. Check for double-wiring on Select Entered/Select Exited.");
                return;
            }
            lastToggleTime = Time.time;

            if (isInsideEncounterSphere) ExitEncounterSphere();
            else EnterEncounterSphere();
        }

        public void EnterEncounterSphere()
        {
            isInsideEncounterSphere = true;

            if (characterController != null) characterController.enabled = false;
            if (groundLocomotionProvider != null) groundLocomotionProvider.enabled = false;
            if (zeroGLocomotion != null) zeroGLocomotion.enabled = true;

            if (xrOriginRoot != null && encounterEntryPoint != null)
                xrOriginRoot.position = encounterEntryPoint.position;

            Debug.Log("EncounterSphereEntryTrigger: Entered Encounter Sphere.");
        }

        public void ExitEncounterSphere()
        {
            isInsideEncounterSphere = false;

            if (zeroGLocomotion != null) zeroGLocomotion.enabled = false;
            if (groundLocomotionProvider != null) groundLocomotionProvider.enabled = true;
            if (characterController != null) characterController.enabled = true;

            if (xrOriginRoot != null && deckEntryPoint != null)
                xrOriginRoot.position = deckEntryPoint.position;

            Debug.Log("EncounterSphereEntryTrigger: Returned to Macro Deck.");
        }
    }
}