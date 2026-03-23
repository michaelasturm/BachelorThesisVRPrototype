using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StudySetupManager : MonoBehaviour
{
    [Header("Input files")]
    public TextAsset latinSquareCsv;
    public TextAsset finishedIds;

    [Header("Participant")]
    public int pid;

    [Header("Study setup")]
    [SerializeField] private int totalConditions = 4;

    void Start()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("pid", pid);
        PlayerPrefs.SetInt("scene counter", 1);

        if (IsFinishedId(pid))
        {
            StopStudy($"Participant ID {pid} is already marked as finished.");
            return;
        }

        if (!LoadConditionOrder())
        {
            StopStudy($"Participant ID {pid} was not found in the Latin square CSV.");
            return;
        }

        PlayerPrefs.SetString("starttime", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
        PlayerPrefs.Save();

        Debug.Log($"PID: {pid}");
        for (int i = 1; i <= totalConditions; i++)
        {
            Debug.Log($"s{i}: {PlayerPrefs.GetInt("s" + i)}");
        }
    }

    private bool IsFinishedId(int idToCheck)
    {
        if (finishedIds == null) return false;

        foreach (string rawLine in finishedIds.text.Split('\n'))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(';');
            if (values.Length == 0) continue;
            if (values[0].Trim().Equals("ID", StringComparison.OrdinalIgnoreCase)) continue;

            if (int.TryParse(values[0].Trim(), out int readId) && readId == idToCheck)
            {
                return true;
            }
        }

        return false;
    }

    private bool LoadConditionOrder()
    {
        if (latinSquareCsv == null) return false;

        foreach (string rawLine in latinSquareCsv.text.Split('\n'))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(';');
            if (values.Length < totalConditions + 1) continue;
            if (values[0].Trim().Equals("ID", StringComparison.OrdinalIgnoreCase)) continue;

            if (!int.TryParse(values[0].Trim(), out int readId)) continue;

            if (readId == pid)
            {
                for (int i = 1; i <= totalConditions; i++)
                {
                    if (int.TryParse(values[i].Trim(), out int sceneIndex))
                    {
                        PlayerPrefs.SetInt("s" + i, sceneIndex);
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        return false;
    }

    private void StopStudy(string message)
    {
        Debug.LogError(message);

#if UNITY_EDITOR
        EditorUtility.DisplayDialog("Study setup error", message, "OK");
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
} 