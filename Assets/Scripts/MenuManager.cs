using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public Transform player;
    public float menuDistanceToPlayer = 2;
    public GameObject menu;
    public InputActionProperty showMenu;

    // Update is called once per frame
    void Update()
    {
        if (showMenu.action.WasPressedThisFrame())
        {
            menu.SetActive(!menu.activeSelf);

            menu.transform.position = player.position + new Vector3(player.forward.x, 0, player.forward.z).normalized * menuDistanceToPlayer;
        }

        menu.transform.LookAt(new Vector3(player.position.x, menu.transform.position.y, player.position.z));
        menu.transform.forward *= -1;
    }
}
