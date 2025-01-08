
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    PlayerLivesManager playerLivesManager;
    //public GameObject StartMenu, SettingsMenu, ExitMenu;
    public List<GameObject> PopUp = new List<GameObject>();
    public Button StartButton, SettingsButton, ExitButton;

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
        // StartButton.onClick.AddListener(PopUp_StartGame);
        // ExitButton.onClick.AddListener(PopUp_quit);
        // SettingsButton.onClick.AddListener(PopUp_Settings);
        //Pause();
    }


    // Popup is called
    public void PopUp_StartGame()
    {
        // Fungsi ini akan di panggil saat button start di klik
        // Digunakan untuk menampilkan popup start game dan menghilangkan popup yang lain
        foreach (GameObject PopUp in PopUp)
        {
            if (PopUp.name == "StartGame")
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
    public void PlayGame()
    {
        // Fungsi ini akan di panggil saat button play di klik
        // Digunakan untuk memuat scene selanjutnya
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        PlayerPrefs.SetInt("Lives", 5);
        // Digunakan untuk mengatur kecepatan game
        Time.timeScale = 1f;
    }
    public void ExitGame()
    {
        // Fungsi ini akan di panggil saat button exit di klik
        // Digunakan untuk keluar dari game
        Application.Quit();
    }

    public void toMainMenu(){
        // Fungsi ini akan di panggil saat button main menu di klik
        SceneManager.LoadScene(0);
        Time.timeScale = 0;
    }

    public void Restart()
    {
        // Fungsi ini akan di panggil saat button restart di klik
        if (PlayerPrefs.GetInt("Lives") <= 0)
        {
            playerLivesManager.ResetLives();
            SceneManager.LoadScene(1);
            Time.timeScale = 1f;
        }
        else
        {
            // Digunakan untuk memuat ulang scene yang sama
            // Dapatkan nama scene yang sedang berjalan
            string currentSceneName = SceneManager.GetActiveScene().name;
            // Muat ulang scene yang sama
            SceneManager.LoadScene(currentSceneName);
            // Digunakan untuk mengatur kecepatan game
            Time.timeScale = 1f;
        }

    }

    public void Resume()
    {
        Time.timeScale = 1f;
    }

    // Fungsi ini digunakan untuk  pause di game
    public void Pause()
    {
        playerLivesManager = GameObject.Find("Player").GetComponent<PlayerLivesManager>();
        if (playerLivesManager != null)
        {
            bool isDead = playerLivesManager.isDead;
            Transform parentTransform = GameObject.Find("onGUI").transform;
            GameObject Pause = parentTransform.Find("Pause").gameObject;
            if (Pause != null && Input.GetKeyDown(KeyCode.Escape) && Time.timeScale == 1f)
            {
                Pause.SetActive(true);
                Time.timeScale = 0f;
            }
            else if (Pause != null && Input.GetKeyDown(KeyCode.Escape) && Time.timeScale == 0f && !isDead)
            {
                Pause.SetActive(false);
                Time.timeScale = 1f;
            }
        }

    }

}

