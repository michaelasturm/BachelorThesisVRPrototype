using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.UI;

public class comfort_test : MonoBehaviour
{

    public ToggleGroup toggle_group;
    private string answer;
    public bool comfort_answered = false;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    
    }

    public void NextQuestion() {
        if (comfort_answered == false) {
            Toggle toggle = toggle_group.ActiveToggles().First();
            answer = toggle.name;
            Debug.Log("thermal comfort: " + toggle.name);
            comfort_answered = true;
            gameObject.SetActive(false);
        }
    }
        

    public string getComfortAnswer() {
        return answer;
    }
}
