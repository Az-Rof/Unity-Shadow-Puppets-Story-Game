using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeDeath : MonoBehaviour
{

    public TextMeshProUGUI countdownText;
    private float countdownTime = 240f;

    void Start()
    {
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        while (countdownTime > 0)
        {
            countdownText.text = "Time Left:\n" + countdownTime.ToString("F2");
            countdownTime -= Time.deltaTime;
            yield return null;
        }

        // Trigger death event
        OnDeath();
    }

    private void OnDeath()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        // Implement death logic here
        if (playerController != null)
        {
            playerController.Stats.currentHealth = -1; // Set player's health to -1
        }
    }
    public void pauseCountdown()
    {
        StopCoroutine(Countdown());
    }
    public void resumeCountdown()
    {
        StartCoroutine(Countdown());
    }
}
