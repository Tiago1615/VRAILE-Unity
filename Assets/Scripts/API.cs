using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.InputSystem;
using System.Text;
using TMPro;

public class API: MonoBehaviour
{

    private AudioClip clip;
    [SerializeField] private TMP_Text chatText;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private AudioSource audioSource;
    private bool isRecording = false;
    private int duration = 10;
    private float time;

    public InputActionProperty talkButton;

    //public UnityEvent OnAudioResponse;
    public Animator animator;

    private const string apiURL = "http://192.168.1.38:8080/stt";

    void Start()
    {
        talkButton.action.performed += _ => RecordingAudio();
        tutorialButton.onClick.AddListener(ClearText);
    }

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
            }
        }
    }

    private void RecordingAudio()
    {
        isRecording = true;
        clip = Microphone.Start(Microphone.devices[0], false, duration, 44100);
    }

    private void EndRecording()
    {
        isRecording = false;
        Microphone.End(Microphone.devices[0]);
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
            Debug.Log(request.error);
            chatText.text = "Error: " + request.error;
        }
        else
        {
            string response = Encoding.UTF8.GetString(request.downloadHandler.data);
            MessageResponse Mres = JsonUtility.FromJson<MessageResponse>(response);
            Debug.Log("Server response: " + Mres.response_text);

            byte[] audioBytes = System.Convert.FromBase64String(Mres.audio);
            ToMp3(new MemoryStream(audioBytes));

            string audioPath = $"file://{Application.persistentDataPath}/audio.mp3";
            using (var www = UnityWebRequestMultimedia.GetAudioClip(audioPath, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                    chatText.text = "Error: " + www.error;
                }
                else
                {
                    var audioClip = DownloadHandlerAudioClip.GetContent(www);
                    //OnAuidioResponse.Invoke();
                    animator.SetTrigger("Talking");
                    audioSource.clip = audioClip;
                    audioSource.Play();
                    StartCoroutine(EndAnimation(audioClip.length));
                }
            }
        }
    }

    private IEnumerator EndAnimation(float duration)
    {
        yield return new WaitForSeconds(duration);
        animator.SetTrigger("Idle");
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

    private void ClearText()
    {
        chatText.text = "";
    }
}
