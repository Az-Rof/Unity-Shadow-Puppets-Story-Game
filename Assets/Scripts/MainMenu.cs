
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    //public GameObject StartMenu, SettingsMenu, ExitMenu;
    public List<GameObject> PopUp = new List<GameObject>();
    // public List<GameObject> Arc = new List<GameObject>();
    public Dictionary<string, string> arcToSceneMap = new Dictionary<string, string>
    {
        { "Anoman's Obong Arc", "Anoman_Lv1" },
        { "Ramayana vs Ravana Arc", "Ramayana_Lv1" },
        
    };


    public Button StartButton, ContinueButton, SettingsButton, ExitButton;

    private void Awake()
    {
        // Fungsi ini akan di panggil saat script di attach ke game object
    }

    private void Start()
    {
        // Fungsi ini akan di panggil saat game di jalankan
    }

    // Update is called once per frame
    void Update()
    {
        // Fungsi ini akan di panggil setiap frame
        // Digunakan untuk menambahkan event listener pada button
        StartButton.onClick.AddListener(PopUp_StartGame);
        ExitButton.onClick.AddListener(PopUp_quit);
        SettingsButton.onClick.AddListener(PopUp_Settings);
        // Pause();
    }


    // Popup is called
    public void PopUp_StartGame()
    {
        // Fungsi ini akan di panggil saat button start di klik
        // Digunakan untuk menampilkan popup start game dan menghilangkan popup yang lain
        foreach (GameObject PopUp in PopUp)
        {
            if (PopUp.name == "StartNewGame")
            {
                PopUp.SetActive(true);
            }
            else
            {
                PopUp.SetActive(false);
            }
        }
    }

    public void PopUp_ContinueGame()
    {
        // Fungsi ini akan di panggil saat button continue di klik
        // Digunakan untuk menampilkan popup continue game dan menghilangkan popup yang lain
        foreach (GameObject PopUp in PopUp)
        {
            if (PopUp.name == "ContinueGame")
            {
                PopUp.SetActive(true);
            }
            else
            {
                PopUp.SetActive(false);
            }
        }
    }
    public void PopUp_quit()
    {
        // Fungsi ini akan di panggil saat button exit di klik
        // Digunakan untuk menampilkan popup exit game dan menghilangkan popup yang lain
        foreach (GameObject PopUp in PopUp)
        {
            if (PopUp.name == "ExitGame")
            {
                PopUp.SetActive(true);
            }
            else
            {
                PopUp.SetActive(false);
            }
        }
    }
    public void PopUp_Settings()
    {
        // Fungsi ini akan di panggil saat button settings di klik
        // Digunakan untuk menampilkan popup settings game dan menghilangkan popup yang lain
        foreach (GameObject PopUp in PopUp)
        {
            if (PopUp.name == "SettingsGame")
            {
                PopUp.SetActive(true);
            }
            else
            {
                PopUp.SetActive(false);
            }
        }
    }

    //Function
    public void PlayGame(string arcName)
    {
        if (arcToSceneMap.TryGetValue(arcName, out string sceneName))
        {
            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
            // PlayerPrefs.SetInt("Lives", 5);
            Time.timeScale = 1f;
        }
        else
        {
            Debug.LogWarning($"Arc not found: {arcName}");
        }
    }
    public void ExitGame()
    {
        // Fungsi ini akan di panggil saat button exit di klik
        // Digunakan untuk keluar dari game
        Application.Quit();
    }
}

