// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using System.IO;
// using System.Runtime.CompilerServices;
// using Unity.VisualScripting;
// using Unity.VisualScripting.Dependencies.Sqlite;
// using UnityEngine;
// using UnityEngine.UI;
// using Unity.VisualScripting.FullSerializer;
// using UnityEditor.Experimental.GraphView;
// using TMPro;

// public class ipq_test : MonoBehaviour
// {

//     public ToggleGroup toggle_group;
//     public TextMeshProUGUI text_anchor_positive;
//     public TextMeshProUGUI text_anchor_negative;
//     public TextMeshProUGUI question_header;
//     //private string file_path = "Assets/Scripts/comfort_test.cs";
//     private IPQ_Question[] questions;
//     private string[] ipq_item_names;
//     public TextAsset csv_file;
//     private string[] ipq_answers;
//     private int current_question = 0;
//     public bool ipq_answered = false;


//     // Start is called before the first frame update
//     void Start()
//     {
//         load_ipq();
//         set_question();
//     }

//     // Update is called once per frame
//     void Update()
//     {

//     }

//     private void load_ipq()
//     {
//         string[] lines = csv_file.text.Split('\n');
        
//         questions = new IPQ_Question[lines.Length];
//         ipq_item_names = new string[lines.Length];
//         ipq_answers = new string[lines.Length];

//         for (int i = 0; i < lines.Length; i++) {
//             string[] values = lines[i].Split(';');

//             if (values.Length == 4) {
//                 ipq_item_names[i] = values[0];
//                 IPQ_Question ipq_question = new IPQ_Question(values[1], values[2], values[3]);
//                 questions[i] = ipq_question;
//                 //questions[i] = new IPQ_Question(values[0], values[1], values[2]);
//             }
//         }

//     }

//     private void set_question_() {
//         question_header.text = questions[current_question].get_question();
//         text_anchor_positive.text = questions[current_question].get_positive_anchor();
//         text_anchor_negative.text = questions[current_question].get_negative_anchor();
//     }

//     public void next_question_()
//     {
//         Toggle toggle = toggle_group.ActiveToggles().First();

//         if (toggle != null && ipq_answered == false) {
//             ipq_answers[current_question] = toggle.name; 
//             //Debug.Log("I: " + current_question + ", answer: " + toggle.name);
//             Debug.Log(ipq_answers[current_question]);

//             current_question += 1;

//             if (current_question > 14) {
//                 ipq_answered = true;
//             }
//             // if current_question == 14 -> dann save as csv

//             // else 
            
//             set_question();
//         }

//         // else display "please select an answer"

//         // Debug.Log("thermal comfort: " + toggle.name);
        
//     }
    
//     public string[] getHeadRow_() {
//         return ipq_item_names;
//     }

//      public string[] getAnswers_() {
//         return ipq_answers;
//     }
// }

// public class IPQ_Question_
// {
//     private string question;
//     private string anchor_negative;
//     private string anchor_positive;

//     public IPQ_Question_(string question, string anchor_negative, string anchor_positive)
//     {
//         this.question = question;
//         this.anchor_negative = anchor_negative;
//         this.anchor_positive = anchor_positive;
//     }

//     public string get_question_() {
//         return question;
//     }
    
//     public string get_positive_anchor() {
//         return anchor_positive;
//     }

//     public string get_negative_anchor() {
//         return anchor_negative;
//     }
// }