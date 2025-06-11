using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Config
{
    public string SERVER_IP;
}

public class ServerConfig : MonoBehaviour
{
    private string IPAddress = "";
    void Start()
    {
        StartCoroutine(LoadIpAddress());
    }

    public IEnumerator LoadIpAddress()
    {
        string configFileName = "config.json";
        string configFilePath = Path.Combine(Application.persistentDataPath, configFileName);

        if (!File.Exists(configFilePath))
        {
            string temporaryConfigPath = Path.Combine(Application.streamingAssetsPath, configFileName);
            string json = "";

            if (temporaryConfigPath.Contains("://") || temporaryConfigPath.Contains(":///"))
            {
                using (UnityWebRequest request = UnityWebRequest.Get(temporaryConfigPath))
                {
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        json = request.downloadHandler.text;
                    }
                    else
                    {
                        Debug.LogError($"Failed to load config file from {temporaryConfigPath}: {request.error}");
                        yield break;
                    }
                }
            }
            else
            {
                json = File.ReadAllText(temporaryConfigPath);
            }

            File.WriteAllText(configFilePath, json);
        }

        string configJson = File.ReadAllText(configFilePath);
        Config config = JsonUtility.FromJson<Config>(configJson);
        if (config != null && !string.IsNullOrEmpty(config.SERVER_IP))
        {
            IPAddress = config.SERVER_IP;
        }
        else
        {
            Debug.Log("Failed to load or parse the config file.");
            IPAddress = "NO_IP";
        }
    }

    public string GetServerIp()
    {
        if (string.IsNullOrEmpty(IPAddress))
        {
            Debug.Log("Server IP is not set. Returning empty string.");
            return "";
        }
        return IPAddress;
    }

}
