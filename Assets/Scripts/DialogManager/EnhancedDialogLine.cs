using UnityEngine;

[System.Serializable]
public class EnhancedDialogLine
{
    [Header("Speaker Info")]
    public string speakerName;
    public Transform speakerTarget;
    
    [Header("Dialog Content")]
    [TextArea(3, 5)]
    public string dialogText;
    
    [Header("Visual Settings")]
    public Color bubbleColor = Color.white;
    public Color textColor = Color.black;
    
    [Header("Timing")]
    public float customDelay = -1f; // -1 uses default delay
    public float customTypeSpeed = -1f; // -1 uses default speed
    
    [Header("Audio")]
    public AudioClip customTypingSound;
    public AudioClip lineCompleteSound;
    
    [Header("Animation")]
    public string characterAnimation;
    public bool shakeOnSpeak = false;
    
    // Validation
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(speakerName) && 
               !string.IsNullOrEmpty(dialogText) && 
               speakerTarget != null;
    }
}
