using System.Collections;
using System.Collections.Generic;
// using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GazeManager : MonoBehaviour
{
    public Camera cam;
    public float minGazeTime = 5.0f;
    public GameObject reticle;
    private float gazeTimer = 0f;
    public Image timeFeedback;
    public GameObject tutorialTarget;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        Vector3 hitPosition = new Vector3(0,0,0);
        bool isTargetHit = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("target"))
            {
                isTargetHit = true;
                hitPosition = hit.point;
            }
        }
        if (isTargetHit)
        {
            if (!reticle.activeSelf)
            {
                reticle.SetActive(true);
            }
            reticle.gameObject.transform.position = hitPosition;
            gazeTimer += Time.deltaTime;
            timeFeedback.fillAmount = gazeTimer / minGazeTime;

            if (gazeTimer >= minGazeTime)
            {
                tutorialTarget.gameObject.SetActive(false);

                // load next scene
                string sceneName = "s" + PlayerPrefs.GetInt("scene counter");
                int nextSceneIndex = PlayerPrefs.GetInt(sceneName);
                SceneManager.LoadScene(nextSceneIndex);
            }

            

        }
        else
        {
            reticle.SetActive(false);
            if (gazeTimer > 0) {
                gazeTimer -= Time.deltaTime;
            }
            timeFeedback.fillAmount = gazeTimer/minGazeTime;
        }


        // if (Physics.Raycast(ray, out hit))
        // {
        //     if (hit.collider.CompareTag("target"))
        //     {
        //         // hit.collider.gameObject.SetActive(false);
        //         // Debug.Log("yes");
        //         if (!reticle.activeSelf)
        //         {
        //             reticle.SetActive(true);
        //         }
        //         reticle.gameObject.transform.position = hit.point;
        //         gazeTimer += Time.deltaTime;
        //         timeFeedback.fillAmount = gazeTimer / minGazeTime;

        //         if (gazeTimer >= minGazeTime)
        //         {
        //             hit.collider.gameObject.SetActive(false);
        //         }
        //     }
        //     else if (!hit.collider.CompareTag("target") && !hit.collider.CompareTag("reticle") && reticle.activeSelf) {
        //         reticle.SetActive(false);
        //     }
        // }
    }

    public void setTutorialEnd()
    {
        tutorialTarget.gameObject.SetActive(true);
    }
}
