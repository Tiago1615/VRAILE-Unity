using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseApp : MonoBehaviour
{
    [SerializeField] private Button quitButton;
    private string apiURL = "";
    public SimAPI api;
    public ServerConfig serverConfig;

    IEnumerator Start()
    {
        if (serverConfig != null)
        {
            yield return new WaitUntil(() => !string.IsNullOrEmpty(serverConfig.GetServerIp()));
            string serverIP = serverConfig.GetServerIp();

            if (serverIP == "NO_IP")
            {
                Debug.LogError("No server IP found. Please check the config file.");
                yield break;
            }

            apiURL = $"http://{serverIP}:8080/";
        }
        quitButton.onClick.AddListener(() =>
        {
            if (api != null)
            {
                StartCoroutine(api.Reboot($"{apiURL}reboot"));
            }
            else
            {
                Debug.LogWarning("SimAPI instance is not assigned.");
            }
            Application.Quit();
        });
    }
}
