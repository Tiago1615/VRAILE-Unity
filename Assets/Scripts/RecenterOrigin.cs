using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RecenterOrigin : MonoBehaviour
{
    public Transform head;
    public Transform origin;
    public Transform target;
    [SerializeField] private InputActionProperty recenterButton;

    void Start()
    {
        StartCoroutine(Recenter());
    }

    void Update()
    {
        if (recenterButton.action.WasPressedThisFrame())
        {
            StartCoroutine(Recenter());
        } 
    }

    public IEnumerator Recenter()
    {
        yield return null;

        Vector3 offset = head.position - origin.position;
        offset.y = 0;
        origin.position = target.position - offset;

        origin.rotation = Quaternion.Euler(0f, 90f, 0f);
    }
}