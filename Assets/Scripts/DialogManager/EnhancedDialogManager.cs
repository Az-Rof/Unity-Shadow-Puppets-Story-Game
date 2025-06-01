using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnhancedDialogManager : MonoBehaviour
{
    [Header("Dialog Setup")]
    public GameObject dialogBubblePrefab;
    public List<EnhancedDialogLine> dialogLines;
    
    [Header("Timing Settings")]
    public float delayBetweenLines = 3f;
    public float typewriterSpeed = 0.05f;
    public bool autoAdvance = true;
    
    [Header("Input Settings")]
    public KeyCode skipKey = KeyCode.Space;
    public KeyCode fastForwardKey = KeyCode.LeftShift;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip typingSound;
    public AudioClip bubblePopSound;
    
    [Header("Events")]
    public UnityEvent OnDialogStart;
    public UnityEvent OnDialogComplete;
    public UnityEvent<int> OnDialogLineChanged;
    
    // Private variables
    private int currentLineIndex = 0;
    private GameObject currentBubble;
    private EnhancedDialogBubble currentBubbleScript;
    private bool isDialogActive = false;
    private bool isTyping = false;
    private bool canAdvance = true;
    private Coroutine dialogCoroutine;
    
    // Properties for external access
    public bool IsActive => isDialogActive;
    public int CurrentLineIndex => currentLineIndex;
    public int TotalLines => dialogLines.Count;
    public float Progress => (float)currentLineIndex / dialogLines.Count;
    
    void Update()
    {
        if (isDialogActive)
        {
            HandleInput();
        }
    }
    
    void HandleInput()
    {
        // Skip/Advance dialog
        if (Input.GetKeyDown(skipKey))
        {
            if (isTyping)
            {
                // Complete current typing immediately
                CompleteCurrentTyping();
            }
            else if (canAdvance)
            {
                // Advance to next line
                AdvanceDialog();
            }
        }
        
        // Fast forward typing
        if (Input.GetKey(fastForwardKey) && isTyping)
        {
            // Increase typing speed temporarily
            if (currentBubbleScript != null)
            {
                currentBubbleScript.typewriterSpeed = typewriterSpeed * 0.1f;
            }
        }
        else if (currentBubbleScript != null)
        {
            // Reset typing speed
            currentBubbleScript.typewriterSpeed = typewriterSpeed;
        }
    }
    
    public void StartDialog()
    {
        if (dialogLines.Count == 0)
        {
            Debug.LogWarning("No dialog lines found!");
            return;
        }
        
        isDialogActive = true;
        currentLineIndex = 0;
        OnDialogStart?.Invoke();
        
        dialogCoroutine = StartCoroutine(PlayDialog());
    }
    
    public void StopDialog()
    {
        if (dialogCoroutine != null)
        {
            StopCoroutine(dialogCoroutine);
        }
        
        if (currentBubble != null)
        {
            Destroy(currentBubble);
        }
        
        isDialogActive = false;
        OnDialogComplete?.Invoke();
    }
    
    public void AdvanceDialog()
    {
        if (!canAdvance) return;
        
        canAdvance = false;
        
        if (currentLineIndex < dialogLines.Count - 1)
        {
            currentLineIndex++;
            StartCoroutine(ShowNextLine());
        }
        else
        {
            EndDialog();
        }
    }
    
    IEnumerator PlayDialog()
    {
        while (currentLineIndex < dialogLines.Count)
        {
            yield return StartCoroutine(ShowCurrentLine());
            
            if (autoAdvance)
            {
                // Wait for typing to complete
                yield return new WaitUntil(() => !isTyping);
                
                // Additional delay
                yield return new WaitForSeconds(delayBetweenLines);
                
                // Auto advance
                if (currentLineIndex < dialogLines.Count - 1)
                {
                    currentLineIndex++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                // Wait for manual advance
                yield return new WaitUntil(() => !canAdvance);
                yield return new WaitForSeconds(0.1f); // Small buffer
                canAdvance = true;
            }
        }
        
        EndDialog();
    }
    
    IEnumerator ShowCurrentLine()
    {
        EnhancedDialogLine line = dialogLines[currentLineIndex];
        
        // Notify line change
        OnDialogLineChanged?.Invoke(currentLineIndex);
        
        // Remove previous bubble
        if (currentBubble != null)
        {
            yield return StartCoroutine(HideBubble(currentBubble));
        }
        
        // Create new bubble
        yield return StartCoroutine(CreateBubble(line));
        
        // Show dialog
        yield return StartCoroutine(ShowDialog(line));
    }
    
    IEnumerator ShowNextLine()
    {
        yield return StartCoroutine(ShowCurrentLine());
        canAdvance = true;
    }
    
    IEnumerator CreateBubble(EnhancedDialogLine line)
    {
        // Instantiate bubble on the character speaker's position
        if (line.speakerTarget == null)
        {
            Debug.LogError("Speaker target is not set for dialog line!");
            yield break;
        }
        
        currentBubble = Instantiate(dialogBubblePrefab, transform);
        currentBubbleScript = currentBubble.GetComponent<EnhancedDialogBubble>();
        
        if (currentBubbleScript == null)
        {
            Debug.LogError("DialogBubble script not found on prefab!");
            yield break;
        }
        
        // Setup bubble
        currentBubbleScript.target = line.speakerTarget;
        currentBubbleScript.typewriterSpeed = typewriterSpeed;
        
        // Play bubble pop sound
        PlaySound(bubblePopSound);
        
        // Small delay for bubble appearance
        yield return new WaitForSeconds(0.1f);
    }
    
    IEnumerator ShowDialog(EnhancedDialogLine line)
    {
        string fullText = $"{line.speakerName}: {line.dialogText}";
        
        isTyping = true;
        
        // Start typing with sound effect
        StartCoroutine(PlayTypingSound());
        
        // Show the dialog
        currentBubbleScript.Show(fullText);
        
        // Wait for typing to complete
        yield return new WaitUntil(() => currentBubbleScript.dialogText.text == fullText);
        
        isTyping = false;
    }
    
    IEnumerator HideBubble(GameObject bubble)
    {
        if (bubble != null)
        {
            // Add fade out animation here if needed
            EnhancedDialogBubble bubbleScript = bubble.GetComponent<EnhancedDialogBubble>();
            
            // Simple fade out (you can enhance this)
            CanvasGroup canvasGroup = bubble.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = bubble.AddComponent<CanvasGroup>();
            
            float fadeTime = 0.3f;
            float elapsed = 0f;
            
            while (elapsed < fadeTime)
            {
                canvasGroup.alpha = 1f - (elapsed / fadeTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            Destroy(bubble);
        }
    }
    
    IEnumerator PlayTypingSound()
    {
        while (isTyping)
        {
            PlaySound(typingSound);
            yield return new WaitForSeconds(typewriterSpeed * 3f); // Adjust sound frequency
        }
    }
    
    void CompleteCurrentTyping()
    {
        if (currentBubbleScript != null && isTyping)
        {
            // Force complete the typewriter effect
            StopAllCoroutines();
            
            EnhancedDialogLine line = dialogLines[currentLineIndex];
            string fullText = $"{line.speakerName}: {line.dialogText}";
            currentBubbleScript.dialogText.text = fullText;
            
            isTyping = false;
            
            // Restart the main dialog coroutine
            if (autoAdvance)
            {
                StartCoroutine(WaitAndAdvance());
            }
        }
    }
    
    IEnumerator WaitAndAdvance()
    {
        yield return new WaitForSeconds(delayBetweenLines);
        
        if (currentLineIndex < dialogLines.Count - 1)
        {
            currentLineIndex++;
            StartCoroutine(ShowCurrentLine());
        }
        else
        {
            EndDialog();
        }
    }
    
    void EndDialog()
    {
        StartCoroutine(FinalCleanup());
    }
    
    IEnumerator FinalCleanup()
    {
        // Hide final bubble
        if (currentBubble != null)
        {
            yield return StartCoroutine(HideBubble(currentBubble));
        }
        
        isDialogActive = false;
        OnDialogComplete?.Invoke();
        
        Debug.Log("Dialog completed!");
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // Public methods for external control
    public void PauseDialog()
    {
        if (dialogCoroutine != null)
        {
            StopCoroutine(dialogCoroutine);
        }
    }
    
    public void ResumeDialog()
    {
        if (isDialogActive && dialogCoroutine == null)
        {
            dialogCoroutine = StartCoroutine(PlayDialog());
        }
    }
    
    public void SetDialogSpeed(float speed)
    {
        typewriterSpeed = speed;
        if (currentBubbleScript != null)
        {
            currentBubbleScript.typewriterSpeed = speed;
        }
    }
    
    public void JumpToLine(int lineIndex)
    {
        if (lineIndex >= 0 && lineIndex < dialogLines.Count)
        {
            currentLineIndex = lineIndex;
            StartCoroutine(ShowCurrentLine());
        }
    }
    
    // Debugging methods
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    void OnGUI()
    {
        if (!isDialogActive) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"Dialog Progress: {currentLineIndex + 1}/{dialogLines.Count}");
        GUILayout.Label($"Is Typing: {isTyping}");
        GUILayout.Label($"Can Advance: {canAdvance}");
        
        if (GUILayout.Button("Skip Line"))
        {
            AdvanceDialog();
        }
        
        if (GUILayout.Button("Complete Typing"))
        {
            CompleteCurrentTyping();
        }
        
        GUILayout.EndArea();
    }
    
    // Auto-start dialog when scene loads
    void Start()
    {
        // Delay start to ensure everything is initialized
        StartCoroutine(AutoStartDialog());
    }
    
    IEnumerator AutoStartDialog()
    {
        yield return new WaitForSeconds(0.5f);
        StartDialog();
        Debug.Log("Auto-started dialog system!");
    }
}