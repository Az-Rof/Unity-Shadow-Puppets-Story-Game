using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    PlayerLivesManager playerLivesManager;
    public GameObject PausePanel;
    public List<GameObject> PopUp = new List<GameObject>();

    // Fungsi ini digunakan untuk  pause di game
    public void pause()
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

    public void RestartGame()
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

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        PausePanel.SetActive(false);
    }

    public void toMainMenu()
    {
        // Fungsi ini akan di panggil saat button main menu di klik
        SceneManager.LoadScene(0);
        Time.timeScale = 0;
    }
    public void PauseGame()
    {

        PausePanel.SetActive(true);
        Time.timeScale = 0;

    }
    public void PopUp_RestartGame()
    {
        // Fungsi ini akan di panggil saat button continue di klik
        // Digunakan untuk menampilkan popup continue game dan menghilangkan popup yang lain
        foreach (GameObject PopUp in PopUp)
        {
            if (PopUp.name == "RestartGame")
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
}
