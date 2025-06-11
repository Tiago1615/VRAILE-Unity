using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class Dialog : MonoBehaviour
{

    public ActionBasedSnapTurnProvider snap;
    public ActionBasedContinuousTurnProvider continuous;
    public LocomotionProvider locomotionProvider;
    public TeleportationProvider teleportationProvider;
    public Transform standingPoint;
    public InputActionProperty endDialog;
    public GameObject tutorialMenu;

    void Start()
    {
        endDialog.action.performed += _ => OnTriggerExit();
    }

    private async void OnTriggerEnter(Collider other)
    {
        tutorialMenu.SetActive(true);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Dialog");

            Transform player = other.transform;

            await Task.Delay(50);

            player.SetPositionAndRotation(standingPoint.position, standingPoint.rotation);

            locomotionProvider.enabled = false;
            teleportationProvider.enabled = false;
            continuous.enabled = false;
            snap.enabled = false;

        }
    }

    public void OnTriggerExit()
    {
        tutorialMenu.SetActive(false);
        locomotionProvider.enabled = true;
        teleportationProvider.enabled = true;
        continuous.enabled = true;
    }

}
