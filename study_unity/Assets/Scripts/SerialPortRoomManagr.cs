using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System;
using System.Drawing.Printing;
using System.IO;
using UnityEngine.SceneManagement;
using System.Net;


public class SerialPortRoomManagr : MonoBehaviour
{
    // source: https://www.hackster.io/raisingawesome/unity-game-engine-and-arduino-serial-communication-12fdd5
    SerialPort sp;
    float next_time;
    // Start is called before the first frame update
    string safe = "";
    private WaitForSeconds readFreq = new WaitForSeconds(1f);
    private string fileName = Application.dataPath + "/CSV-Data/temp.csv";
    private TextWriter tw;
    private int scenecounter;
    private int envIndex;
    private string[] data = new string[2];
    //private float readFreq = 2f;
    private int userId;
    float timePassed = 0f;

    void Start()
    {
        scenecounter = PlayerPrefs.GetInt("scene counter");
        userId = PlayerPrefs.GetInt("pid");
        
        envIndex = PlayerPrefs.GetInt("s" + scenecounter);
        fileName = Application.dataPath + "/CSV-Data/" + userId + "_count" + scenecounter + "_env" + envIndex + "_roomtemperature.csv";

        tw = new StreamWriter(fileName, true);
        string header = "id;scene;timestamp;humidity;temp;";
        tw.WriteLine(header);
        tw.Close();

        string the_com = "";
        next_time = 1f;
        foreach (string mysps in SerialPort.GetPortNames())
        {
            print(mysps);
            if (mysps == "COM4")
            {
                the_com = mysps;
                break;
            }
        }
        if (the_com != "")
        {
            print("Setup port");
            sp = new SerialPort("\\\\.\\" + the_com, 9600);
            if (!sp.IsOpen)
            {
                print("Opening" + the_com + ", baud 9600");
                sp.Open();
                sp.ReadTimeout = 100;
                sp.Handshake = Handshake.None;
                sp.DtrEnable = true;
                if (sp.IsOpen)
                {
                    print("Open");
                    
                }
            }
            else
            {
                print("is allready opened");
            }
        }
        else
        {
            print("the com is empty");
        }

        StartCoroutine(enumData());
        //InvokeRepeating("logData", 1.0f, 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
  /*      timePassed += Time.deltaTime;
        if (timePassed > next_time)
        { 
            logData();
            timePassed = 0;
        } */
    }

    private IEnumerator enumData()
    {
        while (true)
        {
            if (!sp.IsOpen)
            {
                sp.Open();
            }
            if (sp.IsOpen)
            {
                try
                {
                    string msg = sp.ReadLine();
                    safe = msg;
                    printData(msg);
                    Debug.Log(msg);
                }
                catch (TimeoutException)
                { 
                    printData(safe);
                }
            }
            yield return readFreq;
        }
    }
/*
    private void logData()
    {

        if (!sp.IsOpen)
        {
            sp.Open();
            //print("opened sp");
        }
        if (sp.IsOpen)
        {
            try
            {
                string msg = sp.ReadLine();
                safe = msg;
                printData(msg);

            }
            catch (TimeoutException)
            {
                printData(safe);
            }
        }
    }
*/
    private void printData(string msg)
    {
        

        if (msg.Length > 0)
        {
            data = msg.Split(";");
            string dataPoint = userId + ";" + envIndex + ";" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ";" + data[0] + ";" + data[1];
            tw = new StreamWriter(fileName, true);
            tw.WriteLine(dataPoint);
            tw.Close();
        }

    }
}
