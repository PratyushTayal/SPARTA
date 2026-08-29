using UnityEngine;
using UnityEngine.InputSystem;

namespace OrbitGuard.Interaction
{
    public class ZeroGVerticalMovement : MonoBehaviour
    {
        [SerializeField] private Transform playerToMove;
        [SerializeField] private float verticalSpeed = 2f;
        [SerializeField] private InputActionAsset inputActions;

        private InputAction verticalAction;

        private void Awake()
        {
            verticalAction = inputActions.FindActionMap("XRI Left Locomotion").FindAction("Vertical");
        }

        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        private void Update()
        {
            float verticalInput = verticalAction.ReadValue<float>();
            playerToMove.position += Vector3.up * verticalInput * verticalSpeed * Time.deltaTime;
        }
    }
}