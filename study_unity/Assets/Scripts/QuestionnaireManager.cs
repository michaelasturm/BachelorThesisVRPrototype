using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuestionnaireManager : MonoBehaviour
{
    [Header("Study Flow")]
    [SerializeField] private float minWaitTime = 3f;
    [SerializeField] private int totalConditions = 6;
    [SerializeField] private int[] soundEnvironmentIds;

    [Header("Waiting UI")]
    [SerializeField] private Image timeFeedback;
    [SerializeField] private GameObject timeFeedbackUI;
    [SerializeField] private GameObject finishUI;

    [Header("Thermal Comfort UI")]
    [SerializeField] private GameObject comfortUI;
    [SerializeField] private ToggleGroup tgComfort;

    [Header("IPQ UI")]
    [SerializeField] private GameObject ipqUi;
    [SerializeField] private ToggleGroup tgIPQ;
    [SerializeField] private TextMeshProUGUI questionHeader;
    [SerializeField] private TextMeshProUGUI textAnchorNegative;
    [SerializeField] private TextMeshProUGUI textAnchorPositive;
    [SerializeField] private TextAsset csvFile;

    [Header("Sound UI")]
    [SerializeField] private GameObject soundUi;
    [SerializeField] private ToggleGroup tgSound;
    [SerializeField] private TextMeshProUGUI soundQuestionHeader;
    [SerializeField] private TextMeshProUGUI soundAnchorLeft;
    [SerializeField] private TextMeshProUGUI soundAnchorRight;
    [SerializeField] private TextAsset csvFileSound;

    private QuestionnaireItem[] ipqQuestions = Array.Empty<QuestionnaireItem>();
    private QuestionnaireItem[] soundQuestions = Array.Empty<QuestionnaireItem>();

    private string[] ipqItemNames = Array.Empty<string>();
    private string[] soundItemNames = Array.Empty<string>();

    private string[] ipqAnswers = Array.Empty<string>();
    private string[] soundAnswers = Array.Empty<string>();

    private int currentIpqQuestion = 0;
    private int currentSoundQuestion = 0;

    private string comfortAnswer = string.Empty;
    private bool isComfortAnswered = false;
    private bool isIpqAnswered = false;
    private bool isSoundAnswered = false;
    private bool conditionHasSound = false;

    private bool isTransitionPending = false;
    private float timer = 0f;

    private int scenecounter;
    private int envIndex;
    private int userId;
    private string filePath;
    private string filePathFinishedIds;

    private void Start()
    {
        scenecounter = PlayerPrefs.GetInt("scene counter");
        userId = PlayerPrefs.GetInt("pid");
        envIndex = PlayerPrefs.GetInt("s" + scenecounter);

        conditionHasSound = HasSound(envIndex);

        BuildPaths();
        LoadQuestions();
        ResetState();
        PrepareUI();

        if (ipqQuestions.Length == 0)
        {
            Debug.LogError("No IPQ questions loaded. Questionnaire cannot start.");
            return;
        }

        SetIpqQuestion();

        if (conditionHasSound && soundQuestions.Length == 0)
        {
            Debug.LogWarning("This condition expects sound questions, but none were loaded. Sound block will be skipped.");
            conditionHasSound = false;
            FillSoundAnswersWith("NA");
        }
    }

    private void Update()
    {
        if (!isTransitionPending)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timeFeedback != null)
        {
            timeFeedback.fillAmount = minWaitTime > 0f ? Mathf.Clamp01(timer / minWaitTime) : 1f;
        }

        if (timer < minWaitTime)
        {
            return;
        }

        isTransitionPending = false;
        LoadNextScene();
    }

    public void checkComfortQ()
    {
        Toggle toggle = tgComfort.ActiveToggles().FirstOrDefault();
        if (toggle == null)
        {
            Debug.Log("No comfort option selected.");
            return;
        }

        comfortAnswer = GetToggleValue(toggle);
        isComfortAnswered = true;

        comfortUI.SetActive(false);
        ipqUi.SetActive(true);
        ResetToggles(tgComfort);
    }

    public void checkIPQ()
    {
        Toggle toggle = tgIPQ.ActiveToggles().FirstOrDefault();
        if (toggle == null)
        {
            Debug.Log("No IPQ option selected.");
            return;
        }

        ipqAnswers[currentIpqQuestion] = GetToggleValue(toggle);

        if (currentIpqQuestion < ipqQuestions.Length - 1)
        {
            currentIpqQuestion++;
            ResetToggles(tgIPQ);
            SetIpqQuestion();
            return;
        }

        isIpqAnswered = true;
        ipqUi.SetActive(false);
        ResetToggles(tgIPQ);

        BeginSoundBlockOrFinish();
    }

    public void checkSound()
    {
        if (!conditionHasSound)
        {
            Debug.LogWarning("checkSound() was called, but this condition has no sound block.");
            return;
        }

        Toggle toggle = tgSound.ActiveToggles().FirstOrDefault();
        if (toggle == null)
        {
            Debug.Log("No sound option selected.");
            return;
        }

        soundAnswers[currentSoundQuestion] = GetToggleValue(toggle);

        if (currentSoundQuestion < soundQuestions.Length - 1)
        {
            currentSoundQuestion++;
            ResetToggles(tgSound);
            SetSoundQuestion();
            return;
        }

        isSoundAnswered = true;
        soundUi.SetActive(false);
        ResetToggles(tgSound);

        FinishQuestionnaire();
    }

    private void BuildPaths()
    {
        string dir = Path.Combine(Application.persistentDataPath, "CSV-Data");
        string finishedDir = Path.Combine(dir, "FinishedIDs");
        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(finishedDir);

        filePath = Path.Combine(dir, $"{userId}_count{scenecounter}_env{envIndex}_questionnaire.csv");
        filePathFinishedIds = Path.Combine(finishedDir, "finishedIds.csv");
    }

    private void LoadQuestions()
    {
        LoadCsvIntoArrays(csvFile, out ipqQuestions, out ipqItemNames, out ipqAnswers);
        LoadCsvIntoArrays(csvFileSound, out soundQuestions, out soundItemNames, out soundAnswers);
    }

    private void LoadCsvIntoArrays(
        TextAsset source,
        out QuestionnaireItem[] questions,
        out string[] itemNames,
        out string[] answers)
    {
        List<QuestionnaireItem> questionList = new List<QuestionnaireItem>();
        List<string> itemList = new List<string>();

        if (source == null)
        {
            questions = Array.Empty<QuestionnaireItem>();
            itemNames = Array.Empty<string>();
            answers = Array.Empty<string>();
            return;
        }

        foreach (string rawLine in source.text.Split('\n'))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] values = line.Split(';');
            if (values.Length < 4)
            {
                continue;
            }

            if (values[1].Trim().Equals("question", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            itemList.Add(values[0].Trim().TrimStart('\uFEFF'));
            questionList.Add(new QuestionnaireItem(
                values[1].Trim(),
                values[2].Trim(),
                values[3].Trim()
            ));
        }

        questions = questionList.ToArray();
        itemNames = itemList.ToArray();
        answers = new string[questions.Length];
    }

    private void ResetState()
    {
        currentIpqQuestion = 0;
        currentSoundQuestion = 0;

        comfortAnswer = string.Empty;
        isComfortAnswered = false;
        isIpqAnswered = false;
        isSoundAnswered = !conditionHasSound;

        timer = 0f;
        isTransitionPending = false;

        FillAnswersWith(ipqAnswers, string.Empty);
        FillAnswersWith(soundAnswers, conditionHasSound ? string.Empty : "NA");
    }

    private void PrepareUI()
    {
        if (comfortUI != null) comfortUI.SetActive(true);
        if (ipqUi != null) ipqUi.SetActive(false);
        if (soundUi != null) soundUi.SetActive(false);
        if (timeFeedbackUI != null) timeFeedbackUI.SetActive(false);
        if (finishUI != null) finishUI.SetActive(false);

        if (timeFeedback != null)
        {
            timeFeedback.fillAmount = 0f;
        }
    }

    private void SetIpqQuestion()
    {
        if (ipqQuestions.Length == 0 || currentIpqQuestion >= ipqQuestions.Length)
        {
            return;
        }

        questionHeader.text = ipqQuestions[currentIpqQuestion].Question;
        textAnchorNegative.text = ipqQuestions[currentIpqQuestion].AnchorLeft;
        textAnchorPositive.text = ipqQuestions[currentIpqQuestion].AnchorRight;
    }

    private void SetSoundQuestion()
    {
        if (soundQuestions.Length == 0 || currentSoundQuestion >= soundQuestions.Length)
        {
            return;
        }

        soundQuestionHeader.text = soundQuestions[currentSoundQuestion].Question;
        soundAnchorLeft.text = soundQuestions[currentSoundQuestion].AnchorLeft;
        soundAnchorRight.text = soundQuestions[currentSoundQuestion].AnchorRight;
    }

    private void BeginSoundBlockOrFinish()
    {
        if (conditionHasSound && soundQuestions.Length > 0)
        {
            soundUi.SetActive(true);
            SetSoundQuestion();
            return;
        }

        FillSoundAnswersWith("NA");
        isSoundAnswered = true;
        FinishQuestionnaire();
    }

    private void FinishQuestionnaire()
    {
        if (!isComfortAnswered || !isIpqAnswered || !isSoundAnswered)
        {
            Debug.LogWarning("Tried to finish questionnaire before all required parts were answered.");
            return;
        }

        WriteQuestionnaireCSV();

        int nextCounter = PlayerPrefs.GetInt("scene counter");

        if (nextCounter > totalConditions)
        {
            if (finishUI != null)
            {
                finishUI.SetActive(true);
            }

            WriteFinishedID();
            return;
        }

        if (timeFeedbackUI != null)
        {
            timeFeedbackUI.SetActive(true);
        }

        if (timeFeedback != null)
        {
            timeFeedback.fillAmount = 0f;
        }

        timer = 0f;
        isTransitionPending = true;
    }

    private void WriteQuestionnaireCSV()
    {
        bool writeHeader = !File.Exists(filePath);

        using (StreamWriter tw = new StreamWriter(filePath, true))
        {
            if (writeHeader)
            {
                List<string> headerParts = new List<string>
                {
                    "id",
                    "scene",
                    "time",
                    "comfort"
                };

                headerParts.AddRange(ipqItemNames);
                headerParts.AddRange(soundItemNames);

                tw.WriteLine(string.Join(";", headerParts));
            }

            List<string> answerParts = new List<string>
            {
                userId.ToString(),
                envIndex.ToString(),
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                comfortAnswer
            };

            answerParts.AddRange(ipqAnswers);
            answerParts.AddRange(soundAnswers);

            tw.WriteLine(string.Join(";", answerParts));
        }

        scenecounter += 1;
        PlayerPrefs.SetInt("scene counter", scenecounter);
        PlayerPrefs.Save();
    }

    private void WriteFinishedID()
    {
        bool writeHeader = !File.Exists(filePathFinishedIds);

        using (StreamWriter tw = new StreamWriter(filePathFinishedIds, true))
        {
            if (writeHeader)
            {
                tw.WriteLine("ID;starttime;endtime");
            }

            tw.WriteLine(
                PlayerPrefs.GetInt("pid") + ";" +
                PlayerPrefs.GetString("starttime") + ";" +
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
    }

    private void LoadNextScene()
    {
        int nextCounter = PlayerPrefs.GetInt("scene counter");

        if (nextCounter > totalConditions)
        {
            return;
        }

        int nextSceneIndex = PlayerPrefs.GetInt("s" + nextCounter, -1);
        if (nextSceneIndex < 0)
        {
            Debug.LogError($"No scene assigned for s{nextCounter}.");
            return;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    private bool HasSound(int environmentId)
    {
        if (soundEnvironmentIds == null || soundEnvironmentIds.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < soundEnvironmentIds.Length; i++)
        {
            if (soundEnvironmentIds[i] == environmentId)
            {
                return true;
            }
        }

        return false;
    }

    private string GetToggleValue(Toggle toggle)
    {
        return toggle.name.Trim();
    }

    private void FillSoundAnswersWith(string value)
    {
        FillAnswersWith(soundAnswers, value);
    }

    private void FillAnswersWith(string[] answers, string value)
    {
        if (answers == null)
        {
            return;
        }

        for (int i = 0; i < answers.Length; i++)
        {
            answers[i] = value;
        }
    }

    private void ResetToggles(ToggleGroup toggleGroup)
    {
        if (toggleGroup == null)
        {
            return;
        }

        foreach (Toggle toggle in toggleGroup.GetComponentsInChildren<Toggle>(true))
        {
            toggle.isOn = false;
        }
    }

    [Serializable]
    public class QuestionnaireItem
    {
        public string Question { get; private set; }
        public string AnchorLeft { get; private set; }
        public string AnchorRight { get; private set; }

        public QuestionnaireItem(string question, string anchorLeft, string anchorRight)
        {
            Question = question;
            AnchorLeft = anchorLeft;
            AnchorRight = anchorRight;
        }
    }
}
