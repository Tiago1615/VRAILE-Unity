using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Text;
using System.IO;
using System.Threading.Tasks;

[System.Serializable]
public class MessageResponse
{
    public string response_text;
    public string audio;
}

public class WhisperAPI : MonoBehaviour
{

    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private Button recordButton;
    [SerializeField] private TMP_Text chatText;
    private AudioClip clip;
    [SerializeField] private AudioSource audioSource;
    private bool isRecording = false;
    private int duration = 10;
    private float time;

    private const string apiURL = "http://192.168.1.38:8080/stt";


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var device in Microphone.devices)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(device));
            Debug.Log(device);
        }

        recordButton.onClick.AddListener(RecordingAudio);
    }

    private void RecordingAudio()
    {
        isRecording = true;
        recordButton.interactable = false;
        clip = Microphone.Start(dropdown.options[dropdown.value].text, false, duration, 44100);
    }

    private void EndRecording()
    {
        isRecording = false;
        Microphone.End(dropdown.options[dropdown.value].text);
        StartCoroutine(SendAudio(apiURL, clip));
        Destroy(clip);
    }

    private IEnumerator SendAudio(string url, AudioClip clip)
    {
        byte[] data = SaveWav.Save("audio.wav", clip);

        UnityWebRequest request = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(data),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "audio/wav");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            AddErrorMessageToChat("Error: " + request.error);
        }
        else
        {
            string response = Encoding.UTF8.GetString(request.downloadHandler.data);
            MessageResponse Mres = JsonUtility.FromJson<MessageResponse>(response);
            Debug.Log("Server response: " + Mres.response_text);
            AddNPCMessageToChat("NPC: " + Mres.response_text);

            byte[] audioBytes = System.Convert.FromBase64String(Mres.audio);
            ToMp3(new MemoryStream(audioBytes));

            string audioPath = $"file://{Application.persistentDataPath}/audio.mp3";
            using (var www = UnityWebRequestMultimedia.GetAudioClip(audioPath, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    AddErrorMessageToChat("Error: " + www.error);
                }
                else
                {
                    var audioClip = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.clip = audioClip;
                    audioSource.Play();
                }
            }
        }
    }

    private void AddNPCMessageToChat(string message){
        chatText.color = Color.white;
        chatText.text = message + "\n";
        chatText.ForceMeshUpdate();
        //StartCoroutine(ClearChat(10.0f));
    }

    private void AddErrorMessageToChat(string message){
        chatText.color = Color.white;
        chatText.text = message + "\n";
        chatText.ForceMeshUpdate();
        StartCoroutine(ClearChat(10.0f));
    }

    private IEnumerator ClearChat(float delay){
        yield return new WaitForSeconds(delay);
        chatText.text = "";
    }

    private void ToMp3(Stream stream)
    {
        string filePath = $"{Application.persistentDataPath}/audio.mp3";

        using (var audioStream = new FileStream(filePath, FileMode.Create))
        {
            byte[] buffer = new byte[8 * 1024];
            int bytesRead;

            while( (bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0 )
            {
                audioStream.Write(buffer, 0, bytesRead);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (isRecording)
        {
            time += Time.deltaTime;

            chatText.text = "Recording... " + (duration - time).ToString("F2") + "s";

            if (time >= duration)
            {
                time = 0;
                EndRecording();
                chatText.text = "Recording finished";
                recordButton.interactable = true;
            }
        }
    }
}
