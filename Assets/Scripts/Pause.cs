using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Playables;

public class Pause : MonoBehaviour
{

    [Header("Timeline Skip")]
    public PlayableDirector SkipTimeline; // Reference to the PlayableDirector for skipping timeline    
    [Header("Pause Panel")]
    public GameObject PausePanel;
    public GameObject GameOverPanel;
    public List<GameObject> PopUp = new List<GameObject>();
    PlayerController playerController;


    private void FixedUpdate()
    {
        // Check if the player is dead and trigger Game Over
        GameOver();
    }
    // Fungsi ini digunakan untuk  pause di game
    public void pause()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Time.timeScale == 1f)
        {
            PausePanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && Time.timeScale == 0f)
        {
            PausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void RestartGame()
    {
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f; // Resume time before restarting
        }
        // This function will be called when the restart button is clicked
        CharacterStats characterStats = FindObjectOfType<CharacterStats>();
        if (characterStats != null)
        {
            // characterStats.currentHealth = characterStats.maxHealth;
            // characterStats.currentStamina = characterStats.maxStamina;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        PausePanel.SetActive(false);

        // Skip timeline introduction
        if (SkipTimeline != null)
        {
            SkipTimeline.time = SkipTimeline.duration; // Set the timeline to the end
            SkipTimeline.Evaluate(); // Evaluate the timeline to apply changes immediately
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

    public void GameOver()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null && playerController.Stats.currentHealth <= 0)
        {
            Time.timeScale = 0; // Pause the game
            Debug.Log(gameObject.name + " has died. Game Over.");
            // Activate Game Over UI or any other logic
            GameOverPanel.SetActive(true);

        }
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

    // Debugging function to skip the timeline
    public void SkipTimelineIntroduction()
    {
        if (SkipTimeline != null)
        {
            SkipTimeline.time = SkipTimeline.duration; // Set the timeline to the end
            SkipTimeline.Evaluate(); // Evaluate the timeline to apply changes immediately
        }
    }
}

