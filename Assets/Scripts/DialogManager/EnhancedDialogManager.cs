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
    
    [Header("Bubble Management")]
    public bool allowMultipleBubbles = false;
    public int maxSimultaneousBubbles = 3;
    public bool adaptBubbleToCharacter = true;
    
    [Header("Character Detection")]
    public LayerMask characterLayer = 1;
    public float characterDetectionRadius = 2f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip typingSound;
    public AudioClip bubblePopSound;
    
    [Header("Events")]
    public UnityEvent OnDialogStart;
    public UnityEvent OnDialogComplete;
    public UnityEvent<int> OnDialogLineChanged;
    public UnityEvent<Transform> OnSpeakerChanged;
    
    // Private variables
    private int currentLineIndex = 0;
    private List<GameObject> activeBubbles = new List<GameObject>();
    private Dictionary<Transform, GameObject> speakerBubbles = new Dictionary<Transform, GameObject>();
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
        if (isDialogActive && adaptBubbleToCharacter)
        {
            UpdateBubblePositions();
        }
    }
    
    void UpdateBubblePositions()
    {
        // Update positions of all active bubbles to avoid overlapping
        foreach (var bubble in activeBubbles)
        {
            if (bubble != null)
            {
                EnhancedDialogBubble bubbleScript = bubble.GetComponent<EnhancedDialogBubble>();
                if (bubbleScript != null && bubbleScript.target != null)
                {
                    // Check for nearby characters and adjust positioning
                    AdjustBubbleForNearbyCharacters(bubbleScript);
                }
            }
        }
    }
    
    void AdjustBubbleForNearbyCharacters(EnhancedDialogBubble bubble)
    {
        if (bubble.target == null) return;
        
        // Find nearby characters
        Collider[] nearbyCharacters = Physics.OverlapSphere(
            bubble.target.position, 
            characterDetectionRadius, 
            characterLayer
        );
        
        // If there are multiple characters nearby, adjust positioning
        if (nearbyCharacters.Length > 1)
        {
            // Enable smart positioning for crowded areas
            bubble.SetSmartPositioning(true);
            
            // Add custom fallback positions based on nearby characters
            foreach (Collider character in nearbyCharacters)
            {
                if (character.transform != bubble.target)
                {
                    Vector3 avoidanceOffset = (bubble.target.position - character.transform.position).normalized * 2f;
                    avoidanceOffset.y = 2f; // Keep it elevated
                    bubble.AddCustomFallbackPosition(avoidanceOffset);
                }
            }
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
        
        ClearAllBubbles();
        
        isDialogActive = false;
        OnDialogComplete?.Invoke();
    }
    
    void ClearAllBubbles()
    {
        foreach (var bubble in activeBubbles)
        {
            if (bubble != null)
            {
                StartCoroutine(HideBubbleSmooth(bubble));
            }
        }
        
        activeBubbles.Clear();
        speakerBubbles.Clear();
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
                yield return new WaitForSeconds(0.1f);
                canAdvance = true;
            }
        }
        
        EndDialog();
    }
    
    IEnumerator ShowCurrentLine()
    {
        EnhancedDialogLine line = dialogLines[currentLineIndex];
        
        if (!line.IsValid())
        {
            Debug.LogError($"Invalid dialog line at index {currentLineIndex}");
            yield break;
        }
        
        // Notify line change
        OnDialogLineChanged?.Invoke(currentLineIndex);
        OnSpeakerChanged?.Invoke(line.speakerTarget);
        
        // Handle bubble management based on settings
        yield return StartCoroutine(ManageBubbles(line));
        
        // Show dialog
        yield return StartCoroutine(ShowDialog(line));
    }
    
    IEnumerator ManageBubbles(EnhancedDialogLine line)
    {
        if (allowMultipleBubbles)
        {
            // Check if speaker already has a bubble
            if (speakerBubbles.ContainsKey(line.speakerTarget))
            {
                currentBubble = speakerBubbles[line.speakerTarget];
                currentBubbleScript = currentBubble.GetComponent<EnhancedDialogBubble>();
            }
            else if (activeBubbles.Count < maxSimultaneousBubbles)
            {
                // Create new bubble for this speaker
                yield return StartCoroutine(CreateBubble(line));
                speakerBubbles[line.speakerTarget] = currentBubble;
            }
            else
            {
                // Remove oldest bubble and create new one
                GameObject oldestBubble = activeBubbles[0];
                yield return StartCoroutine(HideBubbleSmooth(oldestBubble));
                
                // Remove from speaker dictionary
                Transform oldSpeaker = null;
                foreach (var kvp in speakerBubbles)
                {
                    if (kvp.Value == oldestBubble)
                    {
                        oldSpeaker = kvp.Key;
                        break;
                    }
                }
                if (oldSpeaker != null)
                {
                    speakerBubbles.Remove(oldSpeaker);
                }
                
                yield return StartCoroutine(CreateBubble(line));
                speakerBubbles[line.speakerTarget] = currentBubble;
            }
        }
        else
        {
            // Single bubble mode - remove previous and create new
            if (activeBubbles.Count > 0)
            {
                yield return StartCoroutine(HideBubbleSmooth(activeBubbles[0]));
            }
            
            yield return StartCoroutine(CreateBubble(line));
        }
    }
    
    private GameObject currentBubble;
    private EnhancedDialogBubble currentBubbleScript;
    
    IEnumerator CreateBubble(EnhancedDialogLine line)
    {
        if (line.speakerTarget == null)
        {
            Debug.LogError("Speaker target is not set for dialog line!");
            yield break;
        }
        
        GameObject bubble = Instantiate(dialogBubblePrefab, transform);
        EnhancedDialogBubble bubbleScript = bubble.GetComponent<EnhancedDialogBubble>();
        
        if (bubbleScript == null)
        {
            Debug.LogError("Enhanced DialogBubble script not found on prefab!");
            Destroy(bubble);
            yield break;
        }
        
        // Setup bubble
        bubbleScript.SetTarget(line.speakerTarget);
        bubbleScript.typewriterSpeed = line.customTypeSpeed > 0 ? line.customTypeSpeed : typewriterSpeed;
        
        // Enable smart positioning if character adaptation is enabled
        bubbleScript.SetSmartPositioning(adaptBubbleToCharacter);
        
        // Configure visual effects based on line settings
        bubbleScript.enableShake = line.shakeOnSpeak;
        
        // Add to active bubbles list
        activeBubbles.Add(bubble);
        
        // Store as current bubble
        currentBubble = bubble;
        currentBubbleScript = bubbleScript;
        
        // Play bubble pop sound
        PlaySound(bubblePopSound);
        
        yield return new WaitForSeconds(0.1f);
    }
    
    IEnumerator ShowDialog(EnhancedDialogLine line)
    {
        if (currentBubbleScript == null) yield break;
        
        string fullText = $"{line.speakerName} : {line.dialogText}";
        
        isTyping = true;
        
        // Start typing with sound effect
        StartCoroutine(PlayTypingSound(line.customTypingSound));
        
        // Show the dialog
        currentBubbleScript.Show(fullText);
        
        // Wait for typing to complete
        yield return new WaitUntil(() => currentBubbleScript.dialogText.text == fullText);
        
        isTyping = false;
        
        // Play line complete sound if specified
        if (line.lineCompleteSound != null)
        {
            PlaySound(line.lineCompleteSound);
        }
    }
    
    IEnumerator HideBubbleSmooth(GameObject bubble)
    {
        if (bubble != null)
        {
            EnhancedDialogBubble bubbleScript = bubble.GetComponent<EnhancedDialogBubble>();
            if (bubbleScript != null)
            {
                bubbleScript.Hide();
                yield return new WaitForSeconds(0.5f); // Wait for hide animation
            }
            
            // Remove from lists
            activeBubbles.Remove(bubble);
            
            Destroy(bubble);
        }
    }
    
    IEnumerator PlayTypingSound(AudioClip customSound = null)
    {
        AudioClip soundToPlay = customSound != null ? customSound : typingSound;
        
        while (isTyping)
        {
            PlaySound(soundToPlay);
            yield return new WaitForSeconds(typewriterSpeed * 3f);
        }
    }
    
    IEnumerator ShowNextLine()
    {
        yield return StartCoroutine(ShowCurrentLine());
        canAdvance = true;
    }
    
    void EndDialog()
    {
        StartCoroutine(FinalCleanup());
    }
    
    IEnumerator FinalCleanup()
    {
        // Hide all bubbles smoothly
        List<Coroutine> hideCoroutines = new List<Coroutine>();
        
        foreach (var bubble in activeBubbles.ToArray())
        {
            if (bubble != null)
            {
                hideCoroutines.Add(StartCoroutine(HideBubbleSmooth(bubble)));
            }
        }
        
        // Wait for all bubbles to hide
        foreach (var coroutine in hideCoroutines)
        {
            yield return coroutine;
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
    public void SetMultipleBubblesMode(bool enabled, int maxBubbles = 3)
    {
        allowMultipleBubbles = enabled;
        maxSimultaneousBubbles = maxBubbles;
    }
    
    public void SetCharacterAdaptation(bool enabled)
    {
        adaptBubbleToCharacter = enabled;
    }
    
    public GameObject GetBubbleForSpeaker(Transform speaker)
    {
        return speakerBubbles.ContainsKey(speaker) ? speakerBubbles[speaker] : null;
    }
    
    public void HideBubbleForSpeaker(Transform speaker)
    {
        if (speakerBubbles.ContainsKey(speaker))
        {
            StartCoroutine(HideBubbleSmooth(speakerBubbles[speaker]));
            speakerBubbles.Remove(speaker);
        }
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (!adaptBubbleToCharacter) return;
        
        // Draw character detection radius for each dialog line's speaker
        Gizmos.color = Color.cyan;
        foreach (var line in dialogLines)
        {
            if (line.speakerTarget != null)
            {
                Gizmos.DrawWireSphere(line.speakerTarget.position, characterDetectionRadius);
            }
        }
    }
}