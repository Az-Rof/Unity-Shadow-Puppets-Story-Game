using UnityEngine;

[System.Serializable]
public class EnhancedDialogLine
{
    [Header("Speaker Info")]
    [SerializeField, HideInInspector]
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

    // Property to access speakerName (read-only)
    public string SpeakerName
    {
        get
        {
            // Always return speakerTarget.name if available, else fallback to saved value
            if (speakerTarget != null)
            {
                speakerName = speakerTarget.name;
                return speakerTarget.name;
            }
            return speakerName;
        }
    }

    // Validation
    public bool IsValid()
    {
        // Update speakerName for serialization
        if (speakerTarget != null)
            speakerName = speakerTarget.name;

        return !string.IsNullOrEmpty(SpeakerName) &&
               !string.IsNullOrEmpty(dialogText) &&
               speakerTarget != null;
    }
}