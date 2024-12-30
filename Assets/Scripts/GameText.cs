using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameText : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI collectedCollectablesText, LivesText;
    [SerializeField] private GameObject gameOverScreen;

    private void Start()
    {
        // Inisialiasi Collectables yang telah dikumpulkan
        UpdateCollectablesText();

    }

    private void Update()
    {
        // Melakukan Update Collectables yang telah dikumpulkan
        UpdateCollectablesText(); 

        LivesText.text = PlayerPrefs.GetInt("Lives") + " / 5";
    }

    private void UpdateCollectablesText()
    {
        // Memperbarui jumlah Collectables yang telah dikumpulkan
        collectedCollectablesText.text = Scoring.collectedCollectables.ToString();
    }

    // Fungsi untuk mengupdate UI lives
    void UpdateLivesText(int lives)
    {

    }

    // Fungsi untuk menghandle game over
    public void GameOver()
    {
        // Tampilkan game over screen
        gameOverScreen.SetActive(true);
        // Hentikan game
        Time.timeScale = 0;
    }
}

