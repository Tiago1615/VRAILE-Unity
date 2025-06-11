using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SetTurnMovement : MonoBehaviour
{
    public ActionBasedSnapTurnProvider snap;
    public ActionBasedContinuousTurnProvider continuous;

    public void SetTurnMovementFromIndex(int index)
    {
        if (index == 0)
        {
            continuous.enabled = true;
            snap.enabled = false;
        }
        else if (index == 1)
        {
            continuous.enabled = false;
            snap.enabled = true;
        }
    }
}
