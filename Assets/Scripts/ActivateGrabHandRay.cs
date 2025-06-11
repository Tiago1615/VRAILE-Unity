using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ActivateGrabHandRay : MonoBehaviour
{
    public GameObject leftGrabHandRay;
    public GameObject rightGrabHandRay;

    public XRDirectInteractor leftDirectGrab;
    public XRDirectInteractor rightDirectGrab;

    // Update is called once per frame
    void Update()
    {
        leftGrabHandRay.SetActive(leftDirectGrab.interactablesSelected.Count == 0);
        rightGrabHandRay.SetActive(rightDirectGrab.interactablesSelected.Count == 0);
    }
}
