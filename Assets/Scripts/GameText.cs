using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameText : MonoBehaviour
{
    [SerializeField] public Slider healthSlider, staminaSlider; // Slider yang akan digunakan
    [SerializeField] private GameObject gameOverScreen;

    // Fungsi untuk menghandle game over
    public void GameOver()
    {
        // Tampilkan game over screen
        gameOverScreen.SetActive(true);
        // Hentikan game
        Time.timeScale = 0;
    }
    
}

