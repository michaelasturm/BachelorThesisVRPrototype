using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class HMDTracker : MonoBehaviour
{
    public GameObject HMD;
    [SerializeField] private float sampleInterval = 0.05f;

    private string fileName;
    private int userId;
    private int envIndex;

    void Start()
    {
        int currentScene = PlayerPrefs.GetInt("scene counter");
        userId = PlayerPrefs.GetInt("pid");
        envIndex = PlayerPrefs.GetInt("s" + currentScene);

        string dir = Path.Combine(Application.persistentDataPath, "CSV-Data");
        Directory.CreateDirectory(dir);
        fileName = Path.Combine(dir, $"{userId}_count{currentScene}_env{envIndex}_hmd.csv");

        if (!File.Exists(fileName))
        {
            using (StreamWriter tw = new StreamWriter(fileName, true))
            {
                tw.WriteLine("id;scene;timestamp;x;y;z;rx;ry;rz");
            }
        }

        StartCoroutine(collectCamData());
    }

    private IEnumerator collectCamData()
    {
        WaitForSeconds wait = new WaitForSeconds(sampleInterval);

        while (true)
        {
            if (HMD != null)
            {
                Vector3 pos = HMD.transform.position;
                Vector3 rot = HMD.transform.rotation.eulerAngles;

                using (StreamWriter tw = new StreamWriter(fileName, true))
                {
                    tw.WriteLine(
                        userId + ";" +
                        envIndex + ";" +
                        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ";" +
                        pos.x + ";" + pos.y + ";" + pos.z + ";" +
                        rot.x + ";" + rot.y + ";" + rot.z
                    );
                }
            }

            yield return wait;
        }
    }
}