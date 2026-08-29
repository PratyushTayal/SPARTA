using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SineUIControllerGalaxyMaterials : MonoBehaviour
{
    public Transform prefabHolder;
    public CanvasGroup canvasGroup;

    private Transform[] prefabs;
    private List<Transform> lt;
    private int activeNumber = 0;

    private void Start()
    {
        lt = new List<Transform>();
        prefabs = prefabHolder.GetComponentsInChildren<Transform>(true);

        foreach (Transform tran in prefabs)
        {
            if (tran.parent == prefabHolder)
            {
                lt.Add(tran);
            }
        }

        prefabs = lt.ToArray();
        EnableActive();
    }

    private void Update()
    {
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            canvasGroup.alpha = 1f - canvasGroup.alpha;
        }

        if (Keyboard.current.dKey.wasPressedThisFrame ||
            Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            ChangeEffect(true);
        }

        if (Keyboard.current.aKey.wasPressedThisFrame ||
            Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            ChangeEffect(false);
        }
    }

    // Turn On active VFX Prefab
    public void EnableActive()
    {
        for (int i = 0; i < prefabs.Length; i++)
        {
            prefabs[i].gameObject.SetActive(i == activeNumber);
        }
    }

    // Change active VFX
    public void ChangeEffect(bool bo)
    {
        if (bo)
        {
            activeNumber++;

            if (activeNumber == prefabs.Length)
            {
                activeNumber = 0;
            }
        }
        else
        {
            activeNumber--;

            if (activeNumber == -1)
            {
                activeNumber = prefabs.Length - 1;
            }
        }

        EnableActive();
    }
}