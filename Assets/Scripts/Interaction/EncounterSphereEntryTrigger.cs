// NEW FILE — this is what was missing from Scale_Transition_Lever. Toggling
// it did nothing because this script's actual logic was never written and
// delivered — only referenced. Attach this exact component (already showing
// in your Inspector) and fill in the fields below.

using UnityEngine;

namespace OrbitGuard.Interaction
{
    public class EncounterSphereEntryTrigger : MonoBehaviour
    {
        [Tooltip("The GameObject holding your ZeroGFlightLocomotion component.")]
        public GameObject zeroGLocomotion;

        [Tooltip("Your existing ground-based Continuous Move Provider / Locomotion Mediator GameObject — disabled while in the Encounter Sphere.")]
        public GameObject groundLocomotion;

        [Tooltip("The CharacterController component on your XR Origin — MUST be disabled while ZeroGFlightLocomotion is writing directly to the Transform, or the two will fight each other.")]
        public CharacterController characterController;

        [Tooltip("The XR Origin (or its rig root) to physically move to the Encounter Sphere's location.")]
        public Transform xrOriginRoot;

        [Tooltip("Where to place the player when entering the Encounter Sphere.")]
        public Transform encounterEntryPoint;

        [Tooltip("Where to return the player when exiting back to the Macro Deck.")]
        public Transform deckEntryPoint;

        private bool isInsideEncounterSphere = false;

        /// <summary>Wire this to Scale_Transition_Lever's XRGrabInteractable "Select Exited" event (i.e. fires when the user releases the lever after pulling it).</summary>
        public void Toggle()
        {
            if (isInsideEncounterSphere) ExitEncounterSphere();
            else EnterEncounterSphere();
        }

        public void EnterEncounterSphere()
        {
            isInsideEncounterSphere = true;

            if (characterController != null) characterController.enabled = false;
            if (groundLocomotion != null) groundLocomotion.SetActive(false);
            if (zeroGLocomotion != null) zeroGLocomotion.SetActive(true);

            if (xrOriginRoot != null && encounterEntryPoint != null)
                xrOriginRoot.position = encounterEntryPoint.position;

            Debug.Log("EncounterSphereEntryTrigger: Entered Encounter Sphere — zero-g flight enabled.");
        }

        public void ExitEncounterSphere()
        {
            isInsideEncounterSphere = false;

            if (zeroGLocomotion != null) zeroGLocomotion.SetActive(false);
            if (groundLocomotion != null) groundLocomotion.SetActive(true);
            if (characterController != null) characterController.enabled = true;

            if (xrOriginRoot != null && deckEntryPoint != null)
                xrOriginRoot.position = deckEntryPoint.position;

            Debug.Log("EncounterSphereEntryTrigger: Returned to Macro Deck — ground locomotion re-enabled.");
        }
    }
}