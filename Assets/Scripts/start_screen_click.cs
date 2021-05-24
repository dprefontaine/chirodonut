using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class start_screen_click : MonoBehaviour
{
    //BUTTON TO START GAME
    public Button start_button;

    // Start is called before the first frame update
    void Start() {
        start_button.onClick.AddListener(TaskOnClick);
    }

    // Update is called once per frame
    void Update() {
        
    }

    void TaskOnClick() {
        SceneManager.LoadScene("game_screen",LoadSceneMode.Single);
    }
}
