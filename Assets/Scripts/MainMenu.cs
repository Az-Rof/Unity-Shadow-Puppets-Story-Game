using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    void Start()
    {

    }
    public void StartNewGame()
    {
        PlayerPrefs.SetInt("Lives", 5);
        SceneManager.LoadScene("CutScene_Intro");
        Time.timeScale = 1;
    }

    public void Exit()
    {
        Application.Quit();
    }
}