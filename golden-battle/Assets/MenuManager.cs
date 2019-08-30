using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour{

    public void OnPlayClick() {
        SceneManager.LoadScene("MainBattle");
    }

    public void OnQuitClick() {
        Application.Quit();
    }
   
}
