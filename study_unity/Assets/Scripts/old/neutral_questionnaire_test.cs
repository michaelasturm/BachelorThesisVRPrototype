// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;

// public class neutral_questionnaire_test : MonoBehaviour
// {
//     public ipq_test ipq_manager;
//     public comfort_test comfort_manager;
//     private TextWriter csv_writer;
//     private string file_path = "Assets/CSV-Data" + DateTime.Now + ".csv";
//     // Start is called before the first frame update
//     void Start()
//     {
//         csv_writer = new StreamWriter(file_path, false);

//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }

//     public void WriteQuestionnaireCSV() {
//         string header = "time;" + "comfort;" + string.Join(";", ipq_manager.getHeadRow());
//         csv_writer.WriteLine(header);

//         string answers = DateTime.Now + ";" + comfort_manager.getComfortAnswer() + ";" + 
//                             string.Join(";", ipq_manager.getAnswers());
        
//         csv_writer.WriteLine(answers);
//         csv_writer.Close();
//         Debug.Log("csv should be written now");

//     }
// }
