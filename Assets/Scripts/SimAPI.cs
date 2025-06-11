using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using TMPro;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Text.RegularExpressions;

public class PatientResponse
{
    public int Patient;
    public string message;
    public List<int> correct_answers;
    public List<int> correct_ages;
    public List<int> correct_genders;
    public bool pediatrics;
}

public class PlayerAnswers
{
    public List<string> patology_answers;
    public List<int> age_answers;
    public List<int> gender_answers;
}

public class SimAPI : MonoBehaviour
{
    //===============================================================
    public List<GameObject> patients = new List<GameObject>();
    public GameObject npcOrigin;
    public GameObject npcOriginSon;
    public GameObject npcGoal;
    public GameObject npcGoalMom;
    public GameObject popup;
    public GameObject errorAnswers;
    public GameObject resultsPanel1;
    public GameObject resultsPanel2;
    public Fading fadeScreen;
    public DoorController doorController;
    public ServerConfig serverConfig;
    public Animation doorAnimation;
    //===============================================================
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private TMP_Text errorAnswersText;
    [SerializeField] private TMP_Text patologyScore;
    [SerializeField] private TMP_Text ageScore;
    [SerializeField] private TMP_Text genderScore;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button skipTutorialButton;
    [SerializeField] private Button metricsButton;
    [SerializeField] private GameObject quitMenu;
    [SerializeField] private GameObject afterReboot;
    [SerializeField] private GameObject doneScreen;
    [SerializeField] private GameObject muteObject;
    [SerializeField] private GameObject recordingObject;
    [SerializeField] private Button rebootButton;
    [SerializeField] private Button resultsButton;
    [SerializeField] private Button afterRebootButton;
    [SerializeField] private Button talkButton;
    [SerializeField] private TMP_Dropdown questionDropdown;
    [SerializeField] private TMP_Dropdown ageDropdown;
    [SerializeField] private TMP_Dropdown genderDropdown;
    //===============================================================
    private GameObject currentPatient = null;
    private GameObject sonPatient = null;
    private bool isMomAndSon = false;
    private bool isRecording = false;
    private bool reachedGoal = false;
    private int patientIndex;
    private int duration = 4;
    private int score = 0;
    private float time;
    private List<string> answers = new List<string>();
    private List<int> ages = new List<int>();
    private List<int> genders = new List<int>();
    private AudioClip clip;
    private Image[] results1;
    private Image[] results2;
    private Coroutine currentAudio;
    //===============================================================
    private Animator patientAnimator;
    private AudioSource NPCaudioSource;
    private NPC_IA npcIA;
    //===============================================================
    private Animator sonAnimator;
    private NPC_IA sonNPCIA;
    //===============================================================
    private string apiURL = "";

    IEnumerator Start()
    {
        if (serverConfig != null)
        {
            yield return new WaitUntil(() => !string.IsNullOrEmpty(serverConfig.GetServerIp()));
            string serverIP = serverConfig.GetServerIp();
            if (serverIP == "NO_IP")
            {
                Debug.LogError("No server IP found. Please check the config file.");
                popup.SetActive(true);
                popupText.text = "No se ha encontrado la IP del servidor. Por favor, comprueba el archivo de configuración.";
                StartCoroutine(HidePopupAfterDelay());
                yield break;
            }

            apiURL = $"http://{serverIP}:8080/";
            StartCoroutine(OpenAI_Status($"{apiURL}cehck-status"));
        }
        tutorialButton.onClick.AddListener(() => StartCoroutine(SetUp($"{apiURL}set-up")));
        skipTutorialButton.onClick.AddListener(() => StartCoroutine(SetUp($"{apiURL}set-up")));
        metricsButton.onClick.AddListener(() =>
        {
            bool pathology = AddAnswer(questionDropdown.options[questionDropdown.value].text);
            bool age = AddOtherAnswer(ageDropdown.value, ages);
            bool gender = AddOtherAnswer(genderDropdown.value, genders);
            ClearDropdowns();
            if (pathology && age && gender)
            { 
                StartCoroutine(SetUp($"{apiURL}set-up"));
            }
        });
        resultsButton.onClick.AddListener(() =>
        {
            results1 = GetImages(resultsPanel1);
            results2 = GetImages(resultsPanel2);
            StartCoroutine(SendAnswers($"{apiURL}check-answers"));
        });
        rebootButton.onClick.AddListener(() =>
        {
            StartCoroutine(Reboot($"{apiURL}reboot"));
            afterReboot.SetActive(true);
        });
        afterRebootButton.onClick.AddListener(() => StartCoroutine(SetUp($"{apiURL}set-up")));
        talkButton.onClick.AddListener(() => RecordingAudio());
    }

    void Update()
    {
        if (isRecording)
        {
            time += Time.deltaTime;

            timerText.text = (duration - time).ToString("F2") + "s";

            if (time >= duration)
            {
                time = 0;
                EndRecording();
                timerText.text = "";
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isMomAndSon && other.CompareTag("NPC"))
        {
            Debug.Log("Closing door");
            HandleDoorAnimation("Close");
        }
        else if (isMomAndSon && other.CompareTag("NPC_Son"))
        {
            Debug.Log("Closing door for son");
            HandleDoorAnimation("Close");
        }
    }

    private IEnumerator OpenAI_Status(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                if (request.error.Contains("503"))
                {
                    Debug.Log("OpenAI API is currently unavailable. Please try again later.");
                    popup.SetActive(true);
                    popupText.text = "El servicio de OpenAI está teniendo problemas, esto puede causar problemas en la simulación.";
                    StartCoroutine(HidePopupAfterDelay());
                }
                else
                { 
                    Debug.Log("Error: " + request.error);
                    popup.SetActive(true);
                    popupText.text = "No se pudo comprobar el estado de OpenAI, esto puede causar problemas en la simulación.";
                    StartCoroutine(HidePopupAfterDelay());
                }
            }
        }
    }

    private IEnumerator SetUp(string url)
    {
        if (currentPatient != null)
        {
            Debug.Log("NPC already in scene, removing npc...");
            Debug.Log("Previous patient index: " + patientIndex);
            reachedGoal = false;
            HandleDoorAnimation("Open");

            if (patientIndex == 5)
            {
                SeatAction(currentPatient.GetComponent<Animator>(), sonAnimator, isMomAndSon, "StandUp");

                StartCoroutine(MoveTo(sonNPCIA, sonAnimator, npcOriginSon, npcObj: patients[patientIndex + 1]));
                yield return StartCoroutine(MoveTo(currentPatient.GetComponent<NPC_IA>(), currentPatient.GetComponent<Animator>(), npcOrigin, npcObj: currentPatient));
                ResetPosition(currentPatient, sonPatient);
                StopTalking();
                currentPatient.SetActive(false);
                sonPatient.SetActive(false);
                currentPatient = null;
                sonPatient = null;
                isMomAndSon = false;
            }
            else
            {
                SeatAction(currentPatient.GetComponent<Animator>(), action: "StandUp");

                yield return StartCoroutine(MoveTo(currentPatient.GetComponent<NPC_IA>(), currentPatient.GetComponent<Animator>(), npcOrigin, npcObj: currentPatient));
                ResetPosition(currentPatient);
                StopTalking();
                currentPatient.SetActive(false);
                currentPatient = null;
            }
            HandleDoorAnimation("Close");
            yield return new WaitForSeconds(2f);
        }

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                string response = request.downloadHandler.text;
                PatientResponse PRes = JsonUtility.FromJson<PatientResponse>(response);

                if (PRes.message != "No more patients")
                {
                    HandleDoorAnimation("Open");
                    patientIndex = PRes.Patient;
                    currentPatient = patients[patientIndex];
                    Debug.Log("Current patient index: " + currentPatient.name);
                    if (patientIndex == 5)
                    {
                        isMomAndSon = true;
                        sonPatient = patients[patientIndex + 1];
                        GetDataFromNPC(currentPatient, isMomAndSon);

                        StartCoroutine(MoveTo(npcIA, patientAnimator, npcGoalMom, npcObj: currentPatient));
                        yield return StartCoroutine(MoveTo(sonNPCIA, sonAnimator, npcGoal, npcObj: sonPatient));

                        SeatAction(patientAnimator, sonAnimator, isMomAndSon);
                        reachedGoal = true;
                    }
                    else
                    {
                        GetDataFromNPC(currentPatient);

                        yield return StartCoroutine(MoveTo(npcIA, patientAnimator, npcGoal, npcObj: currentPatient));
                        SeatAction(patientAnimator);

                        reachedGoal = true;
                    }

                    Debug.Log("Patient index: " + patientIndex);
                }
                else
                {
                    Debug.Log("No more patients");
                    doneScreen.SetActive(true);
                }
            }
        }
    }

    private IEnumerator SendAnswers(string url)
    {
        PlayerAnswers wrapper = new PlayerAnswers();
        wrapper.patology_answers = answers;
        wrapper.age_answers = ages;
        wrapper.gender_answers = genders;
        string data = JsonUtility.ToJson(wrapper);

        using (UnityWebRequest request = UnityWebRequest.Post(url, data))
        { 
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
                popup.SetActive(true);
                popupText.text = "Ha ocurrido un error: " + request.error;
                StartCoroutine(HidePopupAfterDelay());
            }
            else
            {
                ClearLists();
                string response = request.downloadHandler.text;
                Debug.Log("Server response: " + response);
                PatientResponse PRes = JsonUtility.FromJson<PatientResponse>(response);

                if (results1 != null && results2 != null)
                {
                    Debug.Log("Updating...");
                    ShowResults(results1, PRes.correct_answers.GetRange(0, 3));
                    ShowResults(results2, PRes.correct_answers.GetRange(3, 3));
                    ageScore.text = $"{CheckScore(PRes.correct_ages)}/6";
                    genderScore.text = $"{CheckScore(PRes.correct_genders)}/6";
                }
            }            
        }
    }

    public IEnumerator Reboot(string url)
    {
        currentPatient = null;
        sonPatient = null;

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log("Rebooting scene");
                fadeScreen.gameObject.SetActive(true);
                score = 0;
                answers.Clear();
                ages.Clear();
                genders.Clear();
                isMomAndSon = false;
                reachedGoal = false;
                fadeScreen.FadeIn(() => { fadeScreen.gameObject.SetActive(false); });
            }
        }
    }

    private IEnumerator MoveTo(NPC_IA npc, Animator animator, GameObject goal = null, GameObject npcObj = null)
    {
        if (goal != null)
        {
            npc.SetGoal(goal);
        }

        NavMeshAgent agent = npcObj.GetComponent<NavMeshAgent>();
        agent.enabled = false;
        agent.enabled = true;

        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;

        npc.move = true;
        animator.SetTrigger("Walking");

        yield return new WaitUntil(() => npc.CheckArrived());

        agent.isStopped = true;
        npc.move = false;

        animator.SetTrigger("Idle");
    }

    private IEnumerator SendAudio(string url, AudioClip clip)
    {
        byte[] data = SaveWav.Save("audio.wav", clip);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "audio/wav");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
                popup.SetActive(true);
                popupText.text = "Ha ocurrido un error: " + request.error;
                StartCoroutine(HidePopupAfterDelay());
            }
            else
            {
                string response = Encoding.UTF8.GetString(request.downloadHandler.data);
                MessageResponse Mres = JsonUtility.FromJson<MessageResponse>(response);
                Debug.Log("Server response: " + Mres.response_text);

                byte[] audioBytes = System.Convert.FromBase64String(Mres.audio);
                currentAudio = StartCoroutine(PlayAudio(audioBytes, NPCaudioSource, patientAnimator));
            }
        }
    }

    private IEnumerator PlayAudio(byte[] bytes, AudioSource audioSource, Animator animator)
    {
        if (bytes == null || bytes.Length == 0)
        {
            Debug.Log("Audio data is empty or null.");
            yield break;
        }
        
        using (MemoryStream stream = new MemoryStream(bytes))
        {
            ToMp3(stream);
        }

        string audioPath = $"file://{Application.persistentDataPath}/audio.mp3";
        using (var www = UnityWebRequestMultimedia.GetAudioClip(audioPath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                popup.SetActive(true);
                popupText.text = "Ha ocurrido un error: " + www.error;
                StartCoroutine(HidePopupAfterDelay());
            }
            else
            {
                var audioClip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = audioClip;
                audioSource.Play();

                float t = 0f;
                while (t < audioClip.length)
                {
                    if (!audioSource.isPlaying)
                    {
                        break;
                    }
                    t += Time.deltaTime;
                    yield return null;
                }
            }
        }
    }

    private IEnumerator HidePopupAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        popup.SetActive(false);
    }

    private void GetDataFromNPC(GameObject npcObj, bool isMomAndSon = false)
    {
        npcObj.SetActive(true);
        patientAnimator = npcObj.GetComponent<Animator>();
        NPCaudioSource = npcObj.GetComponent<AudioSource>();
        npcIA = npcObj.GetComponent<NPC_IA>();

        if (isMomAndSon)
        {
            GameObject son = patients[patientIndex + 1];
            son.SetActive(true);
            sonAnimator = son.GetComponent<Animator>();
            sonNPCIA = son.GetComponent<NPC_IA>();
        }
    }

    private void SeatAction(Animator animator, Animator sonAnimator = null, bool isMomAndSon = false, string action = "SitDown")
    {
        Debug.Log("Seating action: " + action);
        fadeScreen.fadeDuration = 1;
        fadeScreen.gameObject.SetActive(true);

        if (isMomAndSon && sonAnimator != null)
        {
            DoAction(animator, sonAnimator, isMomAndSon, action);
        }
        else
        {
            DoAction(animator, action: action);
        }

        fadeScreen.FadeIn(() =>
        {
            fadeScreen.gameObject.SetActive(false);
        });
    }

    private void DoAction(Animator animator, Animator sonAnimator = null, bool isMomAndSon = false, string action = "SitDown")
    {
        Vector3 position;
        if (isMomAndSon == false)
        {
            position = action == "SitDown" ? new Vector3(9.27f, 0.17f, 9.553f) : new Vector3(9.502377f, 0.02744955f, 8.90118f);
        }
        else
        {
            position = action == "SitDown" ? new Vector3(9.24f, 0.17f, 7.901f) : new Vector3(9.164432f, 0.01911622f, 7.472968f);
        }
        Vector3 sonPosition = action == "SitDown" ? new Vector3(9.276f, 0.303f, 9.515f) : new Vector3(9.513011f, 0.02744955f, 8.906506f);

        GameObject currentPatient = patients[patientIndex];
        NavMeshAgent agent = currentPatient.GetComponent<NavMeshAgent>();

        agent.ResetPath();
        agent.Warp(position - Vector3.up * agent.baseOffset);
        agent.updatePosition = false;
        agent.updateRotation = false;
        if (isMomAndSon == false)
        {
            currentPatient.transform.rotation = Quaternion.Euler(0f, action == "SitDown" ? 248.517f : 0f, 0f);
        }
        else
        { 
            currentPatient.transform.rotation = Quaternion.Euler(0f, action == "SitDown" ? -58.359f : 0f, 0f);
        }
        animator.SetTrigger(action);

        if (isMomAndSon && sonAnimator != null)
        {
            GameObject son = patients[patientIndex + 1];
            NavMeshAgent sonAgent = son.GetComponent<NavMeshAgent>();

            sonAgent.ResetPath();
            sonAgent.Warp(sonPosition - Vector3.up * sonAgent.baseOffset);
            sonAgent.updatePosition = false;
            sonAgent.updateRotation = false;
            son.transform.rotation = Quaternion.Euler(0f, action == "SitDown" ? 248.517f : 241.739f, 0f);
            sonAnimator.SetTrigger(action);
        }
    }

    private void RecordingAudio()
    {
        if (patientIndex != 5 && currentPatient != null && reachedGoal)
        {
            SetTalkingButtonStyle(new Color(110f / 255f, 236f / 255f, 39f / 255f), false, true, false);
            isRecording = true;
            clip = Microphone.Start(Microphone.devices[0], false, duration, 44100);
        }
        else if (patientIndex == 5 && currentPatient != null && sonPatient != null && reachedGoal)
        {
            SetTalkingButtonStyle(new Color(110f / 255f, 236f / 255f, 39f / 255f), false, true, false);
            isRecording = true;
            clip = Microphone.Start(Microphone.devices[0], false, duration, 44100);
        }
        else
        {
            Debug.Log("No patient in scene or not reached goal yet.");
            popup.SetActive(true);
            popupText.text = "¡Debes esperar a que entre un paciente!";
            StartCoroutine(HidePopupAfterDelay());
        }
    }

    private void EndRecording()
    {
        SetTalkingButtonStyle(new Color(233f / 255f, 34f / 255f, 34f / 255f), true, false, true);
        isRecording = false;
        Microphone.End(Microphone.devices[0]);
        StartCoroutine(SendAudio($"{apiURL}stt", clip));
        Destroy(clip);
    }

    private void ToMp3(Stream stream)
    {
        string filePath = $"{Application.persistentDataPath}/audio.mp3";

        using (var audioStream = new FileStream(filePath, FileMode.Create))
        {
            byte[] buffer = new byte[8 * 1024];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                audioStream.Write(buffer, 0, bytesRead);
            }
        }

    }

    private bool AddAnswer(string answer)
    {
        if (string.IsNullOrEmpty(answer) || answer == "=======")
        {
            errorAnswers.SetActive(true);
            return false;
        }
        else if (currentPatient == null || reachedGoal == false)
        {
            errorAnswers.SetActive(true);
            errorAnswersText.text = "¡Debes esperar a que entre un paciente!";
            return false;
        }

        StopTalking();

        string formatedAnswer = Regex.Replace(answer, "<.*?>", "").Trim();

        answers.Add(formatedAnswer);
        Debug.Log("Answer added: " + formatedAnswer);
        return true;
    }

    private bool AddOtherAnswer(int answer, List<int> list)
    {
        if (answer == 0)
        {
            errorAnswers.SetActive(true);
            Debug.Log("No answer selected");
            return false;
        }
        else if (currentPatient == null || reachedGoal == false)
        {
            errorAnswers.SetActive(true);
            errorAnswersText.text = "¡Debes esperar a que entre un paciente!";
            return false;
        }

        int formatedAnswer = answer - 1;
        list.Add(formatedAnswer);
        return true;
    }

    private void ClearLists()
    {
        answers.Clear();
        ages.Clear();
        genders.Clear();
    }

    void ClearDropdowns()
    { 
        questionDropdown.value = 0;
        ageDropdown.value = 0;
        genderDropdown.value = 0;

        questionDropdown.RefreshShownValue();
        ageDropdown.RefreshShownValue();
        genderDropdown.RefreshShownValue();
    }

    private void ShowResults(Image[] results, List<int> correctAnswers)
    {
        for (int i = 0; i < correctAnswers.Count; i++)
        {
            int index = i * 2;
            if (correctAnswers[i] == 1)
            {
                results[index].gameObject.SetActive(true);
                results[index + 1].gameObject.SetActive(false);
                score++;
            }
            else
            {
                results[index].gameObject.SetActive(false);
                results[index + 1].gameObject.SetActive(true);
            }
        }
        patologyScore.text = $"{score}/6";
    }

    private int CheckScore(List<int> correctAnswers)
    {
        int score = 0;
        for (int i = 0; i < correctAnswers.Count; i++)
        {
            _ = correctAnswers[i] == 1 ? score++ : score;
        }
        Debug.Log("Correct answers: " + score);
        return score;
    }

    private Image[] GetImages(GameObject panel)
    {
        Transform imagesParent = panel.transform.Find("===Answers===");
        Image[] results = new Image[imagesParent.childCount];

        for (int i = 0; i < imagesParent.childCount; i++)
        {
            results[i] = imagesParent.GetChild(i).GetComponent<Image>();
        }

        return results;
    }

    private void StopTalking()
    {
        if (currentAudio != null)
        {
            StopCoroutine(currentAudio);
            currentAudio = null;

            if (NPCaudioSource != null && NPCaudioSource.clip != null)
            {
                Debug.Log("Stopping audio playback");
                NPCaudioSource.Stop();
                Destroy(NPCaudioSource.clip);
                NPCaudioSource.clip = null;
            }
        }
    }

    private void ResetPosition(GameObject patient, GameObject sonPatient = null)
    {
        patient.transform.SetPositionAndRotation(new Vector3(7.891f, 0, 13.32f), Quaternion.Euler(0f, 90f, 0f));

        if (sonPatient != null)
        {
            sonPatient.transform.SetPositionAndRotation(new Vector3(6.349f, 0, 13.32f), Quaternion.Euler(0f, 90f, 0f));
        }
    }

    private void HandleDoorAnimation(string action = "Open")
    {

        if (action == "Open")
        {
            //doorController.PlayOpeningSound();
            doorAnimation.Play("Open");
        }
        else
        {
            doorAnimation.Play("Close");
            //doorController.PlayClosingSound();
        }
    }

    private void SetTalkingButtonStyle(Color buttonColor, bool mute, bool recording, bool interactable)
    {
        talkButton.GetComponent<Image>().color = buttonColor;
        recordingObject.SetActive(recording);
        muteObject.SetActive(mute);
        talkButton.interactable = interactable;
    }
}
