using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System;
using System.Drawing.Printing;



public class RoomTemperature : MonoBehaviour
{
    // source: https://www.hackster.io/raisingawesome/unity-game-engine-and-arduino-serial-communication-12fdd5
    SerialPort sp;
    float next_time;
    // Start is called before the first frame update
    string safe = "";

    void Start()
    {
        string the_com="";
        next_time = Time.time;
        foreach (string mysps in SerialPort.GetPortNames())
        {
            print(mysps);
            if (mysps == "COM3")
            {
                the_com = mysps;
                break;
            }
        }
        if(the_com != "")
        {
            print("Setup port");
            sp = new SerialPort("\\\\.\\" + the_com, 9600);
            if(!sp.IsOpen)
            {
                print("Opening" + the_com + ", baud 9600");
                sp.Open();
                sp.ReadTimeout = 100;
                sp.Handshake = Handshake.None;
                if(sp.IsOpen)
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
    }

    // Update is called once per frame
    void Update()
    {
        //if (Time.time > next_time) {
            if (!sp.IsOpen)
            {
                sp.Open();
                print("opened sp");
            }
            if (sp.IsOpen)
            {
                try{
                    string msg = sp.ReadLine();
                    safe = msg;
                    printData(msg);
                    //printDummyData(msg);
                } catch(TimeoutException){
                    printData(safe);
                    //printDummyData(safe);
                }
            }
            //next_time = Time.time + 5;
        //}
    }

    private void printDummyData(string msg)
    {
        Debug.Log(msg);
    }

    void printData(string msg)
    {
        string regexHum = @"Humidity: [0-9]+.[0-9]+";
        Regex humRegex = new Regex(regexHum);
        string regexTemp = @"Temperature: [0-9]+.[0-9]+";
        Regex tempRegex = new Regex(regexTemp);
        string regexPress = @"Pressure: [0-9]+.[0-9]+";
        Regex pressRegex = new Regex(regexPress);

        MatchCollection matchesHum = humRegex.Matches(msg);
        MatchCollection matchesTemp = tempRegex.Matches(msg);
        MatchCollection matchesPress = pressRegex.Matches(msg);

        if(matchesHum.Count == 1 || matchesTemp.Count == 1 || matchesPress.Count == 1) 
            {
                print(matchesHum[0]);
                print(matchesTemp[0]);
                print(matchesPress[0]);
            }
            else
            {
                print("Something went wrong in the RegEx");
            }
    }
}
