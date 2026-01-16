using System;
using System.Collections;
using System.Collections.Generic;
// using System.Diagnostics;
using System.IO;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEditor;
using UnityEngine;

public class StudySetupManager : MonoBehaviour
{
    // Latin Square csv load
    public TextAsset latinSquareCsv;
    public TextAsset finishedIds;
    public int pid;
    private TextWriter tw;

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.DeleteAll();
        // set PlayerID
        PlayerPrefs.SetInt("pid", pid);  
        // save sceneCounter PlayerPrefs.SetInt(sceneCounter, 0)
        PlayerPrefs.SetInt("scene counter", 1);
        // load csv/scene reihenfolge corresponding to pid
        loadCSV();
        // print ID, counter + scenereihenfolge
        checkIDs();
    }

    private void checkIDs()
    {
        string[] lines = finishedIds.text.Split('\n');

        foreach (string line in lines)
        {
            string[] values = line.Split(";");
            if (values[0] == "ID")
            {
                continue;
            }

            int readID = int.Parse(values[0]);

            if (readID == pid)
            {
                EditorUtility.DisplayDialog("Caution!", "ID already finished, please update it!", "OK, I will check ID on paper and update it!");
                EditorApplication.isPlaying = false;
                break;
            }

        }

        PlayerPrefs.SetString("starttime", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
    }

    private void loadCSV() {
        string[] lines = latinSquareCsv.text.Split('\n');

    	foreach (string line in lines) {

            string[] values = line.Split(";");
            if (values[0] == "ID") {
                continue;
            }

            int readID = int.Parse(values[0]);

            if (readID == pid) {
                Debug.Log("csv line: " + line);

                PlayerPrefs.SetInt("s1", int.Parse(values[1]));
                PlayerPrefs.SetInt("s2", int.Parse(values[2]));
                PlayerPrefs.SetInt("s3", int.Parse(values[3]));
                PlayerPrefs.SetInt("s4", int.Parse(values[4]));
                //PlayerPrefs.SetInt("s5", int.Parse(values[5]));
                //PlayerPrefs.SetInt("s6", int.Parse(values[6]));

                Debug.Log(PlayerPrefs.GetInt("s1"));
                Debug.Log(PlayerPrefs.GetInt("s2"));
                Debug.Log(PlayerPrefs.GetInt("s3"));
                Debug.Log(PlayerPrefs.GetInt("s4"));
                //Debug.Log(PlayerPrefs.GetInt("s5"));
                //Debug.Log(PlayerPrefs.GetInt("s6"));


                break;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
