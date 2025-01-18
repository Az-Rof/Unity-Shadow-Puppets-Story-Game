using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerLivesManager : MonoBehaviour
{
    GameText gameText;
    public static int lives { get; private set; }
    public bool isDead;
    public int startingLives = 5;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Fungsi untuk mengurangi nyawa
    public void ReduceLives()
    {
        gameText = GameObject.FindObjectOfType<GameText>();
        int lives = PlayerPrefs.GetInt("Lives");
        lives = lives - 1;
        PlayerPrefs.SetInt("Lives", lives);
        isDead = true;
        
        Time.timeScale = 0;
        gameText.GameOver();
    }

    // Fungsi untuk mereset nyawa ke jumlah awal
    public void ResetLives()
    {
        PlayerPrefs.SetInt("Lives", startingLives);
    }

}

