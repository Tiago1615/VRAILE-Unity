using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Animation doorAnimation;
    [SerializeField] private AudioSource openDoorSound;
    [SerializeField] private AudioSource closeDoorSound;

    public void PlayOpeningSound()
    {
        if (openDoorSound != null)
        {
            openDoorSound.volume = 200f;
            openDoorSound.Play();
        }
        else
        {
            Debug.LogWarning("Open door sound not assigned.");
        }
    }

    public void PlayClosingSound()
    {
        if (closeDoorSound != null)
        {
            closeDoorSound.PlayDelayed(0.9f);
        }
        else
        {
            Debug.LogWarning("Close door sound not assigned.");
        }
    }
}
