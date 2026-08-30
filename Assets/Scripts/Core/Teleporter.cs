using UnityEngine;
using UnityEngine.InputSystem;

public class Teleporter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform xrRig;
    [SerializeField] private Transform satellite;

    [Header("Teleport Locations")]
    [SerializeField] private Transform[] teleportLocations = new Transform[1];

    [Header("Satellite Teleport Offset")]
    [SerializeField] private Vector3 satelliteOffset = new Vector3(0f, 2f, -3f);

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    private InputAction teleportNext;
    private InputAction teleportPrev;

    private int currentLocation = 0;

    private void Awake()
    {
        // Insert satellite at index 0
        Transform[] newLocations = new Transform[teleportLocations.Length + 1];

        newLocations[0] = satellite;

        for (int i = 0; i < teleportLocations.Length; i++)
        {
            newLocations[i + 1] = teleportLocations[i];
        }

        teleportLocations = newLocations;

        // Get input actions
        InputActionMap locomotionMap =
            inputActions.FindActionMap("XRI Left Locomotion");

        teleportNext = locomotionMap.FindAction("TeleportNext");
        teleportPrev = locomotionMap.FindAction("TeleportPrev");
    }

    private void OnEnable()
    {
        teleportNext.Enable();
        teleportPrev.Enable();
    }

    private void OnDisable()
    {
        teleportNext.Disable();
        teleportPrev.Disable();
    }

    private void Update()
    {
        if (teleportNext.WasPressedThisFrame())
        {
            currentLocation++;

            if (currentLocation >= teleportLocations.Length)
                currentLocation = 0;

            TeleportTo(currentLocation);
        }

        if (teleportPrev.WasPressedThisFrame())
        {
            currentLocation--;

            if (currentLocation < 0)
                currentLocation = teleportLocations.Length - 1;

            TeleportTo(currentLocation);
        }
    }

    private void TeleportTo(int index)
    {
        Transform target = teleportLocations[index];

        if (index == 0)
        {
            // Offset is relative to the satellite's rotation
            Vector3 offsetPosition = target.TransformPoint(satelliteOffset);

            xrRig.position = offsetPosition;
            xrRig.rotation = target.rotation;
        }
        else
        {
            xrRig.position = target.position;
            xrRig.rotation = target.rotation;
        }
    }
}