using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleCanvas : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GameObject uiCanvas;
    private InputAction toggle;
    private bool active = true;
    private void Awake()
    {
        InputActionMap locomotionMap = inputActions.FindActionMap("XRI Right Locomotion");

        toggle = locomotionMap.FindAction("Toggle");
    }

    private void OnEnable()
    {
        toggle.Enable();
    }

    private void OnDisable()
    {
        toggle.Disable();
    }

    private void Update()
    {
        if(toggle.WasPressedThisFrame())
        {
            active = !active;
            uiCanvas.SetActive(active);
        }
    }
}
