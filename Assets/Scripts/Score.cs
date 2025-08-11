using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Score : MonoBehaviour
{
    public static Score instance;

    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI highscoreText;

    private int totalScore;
    private int highscore;
    private int currentLevelScore;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        highscore = PlayerPrefs.GetInt("Highscore", 0);
        UpdateUIReferences();
        UpdateScoreText();
        UpdateHighscoreText();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateUIReferences();

        if (scene.buildIndex == 1) // Level 1
        {
            totalScore = 0;
            currentLevelScore = 0;
        }
        else
        {
            currentLevelScore = 0; // Reset hanya untuk level yang baru dimasuki
        }

        UpdateScoreText();
        UpdateHighscoreText();
    }

    private void UpdateUIReferences()
    {
        GameObject scoreObj = GameObject.Find("Score");
        GameObject highscoreObj = GameObject.Find("Highscore");

        if (scoreObj != null)
            scoreText = scoreObj.GetComponent<TextMeshProUGUI>();

        if (highscoreObj != null)
            highscoreText = highscoreObj.GetComponent<TextMeshProUGUI>();
    }

    public void IncreaseScore(int amount)
    {
        totalScore += amount;
        currentLevelScore += amount;

        if (totalScore > highscore)
        {
            highscore = totalScore;
            PlayerPrefs.SetInt("Highscore", highscore);
            UpdateHighscoreText();
        }

        UpdateScoreText();
    }

    public void RestartCurrentLevel()
    {
        // Kurangi total score dengan score level ini
        totalScore -= currentLevelScore;
        if (totalScore < 0) totalScore = 0;
        currentLevelScore = 0;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {totalScore}";
    }

    private void UpdateHighscoreText()
    {
        if (highscoreText != null)
            highscoreText.text = $"Highscore: {highscore}";
    }
}
