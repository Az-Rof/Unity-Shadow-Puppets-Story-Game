using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameRateManager : MonoBehaviour
{
    [Header("Target Frame Rate")]
    [Tooltip("Set target frame rate for the game. Example: 90")]
    [SerializeField] int targetFrameRate = 75;

    void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0; // Disable vSync to allow targetFrameRate to take effect
    }

    void OnValidate()
    {
        Application.targetFrameRate = targetFrameRate;
    }
}
