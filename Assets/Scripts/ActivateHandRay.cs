using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ActivateHandRay : MonoBehaviour
{
    public GameObject rightHandRay;
    public GameObject leftHandRay;
    public InputActionProperty rightHandRayAction;
    public InputActionProperty leftHandRayAction;
    public InputActionProperty cancelRightHandRayAction;
    public InputActionProperty cancelLeftHandRayAction;

    public XRRayInteractor rightRay;
    public XRRayInteractor leftRay;

    // Update is called once per frame
    void Update()
    {
        bool rightRayHovering = rightRay.TryGetHitInfo(out Vector3 rightPos, out Vector3 rightNormal, out int rightNumber, out bool rightValid);
        bool leftRayHovering = leftRay.TryGetHitInfo(out Vector3 leftPos, out Vector3 leftNormal, out int leftNumber, out bool leftValid);

        rightHandRay.SetActive(!rightRayHovering && cancelRightHandRayAction.action.ReadValue<float>() == 0 && rightHandRayAction.action.ReadValue<float>() > 0.1f);
        leftHandRay.SetActive(!leftRayHovering && cancelLeftHandRayAction.action.ReadValue<float>() == 0 && leftHandRayAction.action.ReadValue<float>() > 0.1f);
    }
}
