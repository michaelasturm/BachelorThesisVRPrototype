using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnvGameManager : MonoBehaviour
{
    // Target stuff
    [Header("TargetManager")]
    private float targetTimout = 7f; // TODO 7f
    private float targetTimer = 0f;
    public GameObject targetHolder;
    private int numTargets;
    // private bool isTargetVisibile = false;
    private bool isTargetTimeout = false;
    private bool isTargetVisible = false;


    // Gaze stuff
    [Header("Gaze Interaction Manager")]
    private Vector3 hitPosition = new Vector3(0, 0, 0);
    public Camera cam;
    public GameObject reticle;
    private float gazeTimer = 0f;
    private float minGazeTime = 5f;
    private Image timeFeedback;

    // questionnaire stuff
    [Header("Questionnaire Manager")]
    private float questionnaireTimeInterval = 90f; // TODO 90
    private int questionnaireCount = 0; // TODO
    private bool isQuestionaireDone = false;
    public Slider slider;
    public GameObject thermalSensationUI;
    private bool isQuestionTime = false;
    private float[] thermalSensationScores = new float[6];
    private long[] questionTimestamps = new long[6];


    // Scene Manager
    private float sceneTimer = 0f;
    private float questionTimer = 0f;
    private float maxSceneTime = 600f; // TODO 10 minutes
    private TextWriter tw;
    private string filePath = Application.dataPath + "/CSV-Data/env_.csv";
    public GameObject leftControllerRay;
    public GameObject rightControllerRay;
    int envIndex;
    int userId;


    // Start is called before the first frame update
    void Start()
    {
        leftControllerRay.gameObject.SetActive(false);
        rightControllerRay.gameObject.SetActive(false);
        numTargets = targetHolder.transform.childCount;
        int currentScene = PlayerPrefs.GetInt("scene counter");
        userId = PlayerPrefs.GetInt("pid");
        envIndex = PlayerPrefs.GetInt("s"+currentScene);
        Debug.Log("counter: " + currentScene + ", index: " + envIndex);
        filePath = Application.dataPath + "/CSV-Data/" + userId + "_count" + currentScene + "_env" + envIndex + "_sensation.csv" ;
    }

    // Update is called once per frame
    void Update()
    {
        // check if its questionnaire time or scene change
        checkSceneTime();

        if (!isQuestionTime)
        {
            // check sight and gaze interaction
            checkRaycast();

            // check target stuff (is it shown, is it timeout)
            checkTargets();
            // target shizzle
        }

    }

    private void checkTargets()
    {

        if (isTargetTimeout)
        {
            // wait for x seconds 
            targetTimer += Time.deltaTime;
            if (targetTimer > targetTimout)
            {
                isTargetTimeout = false;
                targetTimer = 0f;
            }
        }
        else if (!isTargetVisible)
        {
            // show random target
            targetHolder.transform.GetChild(UnityEngine.Random.Range(0, numTargets)).gameObject.SetActive(true);
            isTargetVisible = true;
        }



    }

    private void checkSceneTime()
    {
        sceneTimer += Time.deltaTime;
        questionTimer += Time.deltaTime;

        // do things that must be done when its no question time
        if (questionTimer >= questionnaireTimeInterval)
        {
            // Show the questionnaire
            Debug.Log("Question Time!");
            resetTargets();
            ShowQuestionnaire();
            isQuestionTime = true;
            // Reset sceneTimer for the next interval
            questionTimer = 0f;
        }

        if (sceneTimer >= maxSceneTime && isQuestionaireDone)
        {
            // scene change
            // TODO 5
            SceneManager.LoadScene(5); // TODO
        }
    }

    private void ShowQuestionnaire()
    {
        //maybe reset value of slider
        slider.value = 50;
        thermalSensationUI.SetActive(true);
        leftControllerRay.gameObject.SetActive(true);
        rightControllerRay.gameObject.SetActive(true);
    }

    // called when UI-Button is pressed 
    public void checkQuestionnaireDone()
    {
        // save
        // set avtive false
        if (questionnaireCount <= 5)
        {

            thermalSensationScores[questionnaireCount] = slider.value;
            questionTimestamps[questionnaireCount] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            questionnaireCount += 1;
        }

        // 6th questionnaire taken
        if (questionnaireCount > 5)
        {
            writeCSV();
            isQuestionaireDone = true;
        }
        thermalSensationUI.SetActive(false);
        leftControllerRay.gameObject.SetActive(false);
        rightControllerRay.gameObject.SetActive(false);
        isQuestionTime = false;
    }

    private void writeCSV()
    {
        string header = "id;scene;thermal_sensation_90; thermal_sensation_180; thermal_sensation_270; thermal_sensation_360; thermal_sensation_450;thermal_sensation_540; time_90; time_180; time_270; time_360; time_450; time_540";
        tw = new StreamWriter(filePath, true);
        tw.WriteLine(header);

        string answers = string.Join(";", thermalSensationScores);
        string timestamps = string.Join(";", questionTimestamps);
        tw.WriteLine(userId + ";"+ envIndex + ";" + answers + ";" + timestamps);
        tw.Close();
        Debug.Log("csv written");
    }

    private void checkRaycast()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        // Vector3 hitPosition = new Vector3(0, 0, 0);
        bool isTargetHit = checkTargetHit(hits);


        if (isTargetHit)
        {
            startGaze();
        }
        else
        {
            resetGaze();
        }
    }

    private bool checkTargetHit(RaycastHit[] hits)
    {
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("target"))
            {
                hitPosition = hit.point;
                timeFeedback = hit.transform.gameObject.GetComponentInChildren<Image>();
                return true;
            }
        }
        return false;
    }

    private void startGaze()
    {
        if (!reticle.activeSelf)
        {
            reticle.SetActive(true);
        }
        reticle.gameObject.transform.position = hitPosition;
        gazeTimer += Time.deltaTime;
        if (timeFeedback != null)
        {
            timeFeedback.fillAmount = gazeTimer / minGazeTime;
        }


        if (gazeTimer >= minGazeTime)
        {
            resetTargets();
            // isTargetVisibile = false;

        }
    }

    private void resetTargets()
    {
        foreach (Transform target in targetHolder.gameObject.transform)
        {
            // targetHolder.gameObject.SetActive(false);
            if (target.gameObject.activeInHierarchy)
            {
                target.gameObject.SetActive(false);
                gazeTimer = 0f;
            }
        }
        isTargetTimeout = true;
        isTargetVisible = false;
    }

    private void resetGaze()
    {
        reticle.SetActive(false);
        if (gazeTimer > 0)
        {
            gazeTimer -= Time.deltaTime;
        }
        if (timeFeedback != null)
        {
            timeFeedback.fillAmount = gazeTimer / minGazeTime;
        }

    }

}
