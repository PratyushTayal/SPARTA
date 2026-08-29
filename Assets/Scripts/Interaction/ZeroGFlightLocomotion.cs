using UnityEngine;
using UnityEngine.InputSystem;

namespace OrbitGuard.Interaction
{
    public class ZeroGFlightLocomotion : MonoBehaviour
    {
        public Transform xrOriginToMove;
        public Transform directionSource;
        public float moveSpeedMetersPerSecond = 0.6f;
        public InputActionReference moveAction;
        public InputActionReference verticalAction;

        private void OnEnable()
        {
            moveAction?.action.Enable();
            verticalAction?.action.Enable();
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
            verticalAction?.action.Disable();
        }

        private void Update()
        {
            if (xrOriginToMove == null || directionSource == null) return;

            Vector2 moveInput = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
            float verticalInput = verticalAction != null ? verticalAction.action.ReadValue<float>() : 0f;

            if (moveInput.sqrMagnitude < 0.0001f && Mathf.Abs(verticalInput) < 0.0001f) return;

            Vector3 forward = directionSource.forward;
            Vector3 right = directionSource.right;
            Vector3 up = Vector3.up;

            Vector3 moveDirection = (forward * moveInput.y) + (right * moveInput.x) + (up * verticalInput);

            xrOriginToMove.position += moveDirection * moveSpeedMetersPerSecond * Time.deltaTime;
        }
    }
}
