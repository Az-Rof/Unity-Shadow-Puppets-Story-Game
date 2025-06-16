using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class EnhancedDialogBubble : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 preferredOffset = new Vector3(0, 3f, 0);
    public float minDistanceFromCharacter = 2f;
    public LayerMask obstacleLayerMask = 1; // What layers to avoid

    [Header("Smart Positioning")]
    public bool enableSmartPositioning = true;
    public float characterBoundsCheckRadius = 1.5f;
    public Vector3[] fallbackPositions = new Vector3[]
    {
        new Vector3(0, 3f, 0),    // Above (default)
        new Vector3(2f, 2f, 0),  // Top-right
        new Vector3(-2f, 2f, 0), // Top-left
        new Vector3(2f, 0, 0),   // Right
        new Vector3(-2f, 0, 0),  // Left
        new Vector3(0, -1f, 0)   // Below (last resort)
    };

    [Header("UI Components")]
    public TextMeshProUGUI dialogText;
    public Canvas parentCanvas;
    public RectTransform bubbleBackground;

    [Header("Animation")]
    public float typewriterSpeed = 0.05f;
    public AnimationCurve appearCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float appearDuration = 0.5f;

    [Header("Visual Effects")]
    public bool enableShake = false;
    public float shakeIntensity = 0.1f;
    public bool enableBounce = true;
    public float bounceHeight = 0.2f;
    public float bounceSpeed = 2f;

    [Header("Adaptive Sizing")]
    public bool enableAdaptiveSizing = true;
    public Vector2 minBubbleSize = new Vector2(100, 50);
    public Vector2 maxBubbleSize = new Vector2(400, 200);
    public float paddingX = 20f;
    public float paddingY = 15f;

    // This will be set by the EnhancedDialogManager or default to Camera.main
    private Camera mainCam;
    private Vector3 originalPosition;
    private Vector3 targetWorldPosition;
    private Coroutine typewriterCoroutine;
    private bool isTypingComplete = false;
    private Collider targetCollider;
    private Renderer targetRenderer;
    private float bounceTimer = 0f;

    public void SetRenderCamera(Camera cameraToUse)
    {
        this.mainCam = cameraToUse;
    }

    void Start()
    {
        // Fallback if SetRenderCamera was not called or called with null
        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null && Application.isPlaying)
                Debug.LogWarning("EnhancedDialogBubble: Render camera not set and Camera.main not found.", this);
        }
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();

        if (target != null)
        {
            targetCollider = target.GetComponent<Collider>();
            targetRenderer = target.GetComponent<Renderer>();
        }

        // Start with scale 0 for appear animation
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        if (target != null && mainCam != null)
        {
            UpdatePosition();
        }

        if (enableShake && !isTypingComplete)
        {
            ApplyShakeEffect();
        }

        if (enableBounce && !isTypingComplete)
        {
            ApplyBounceEffect();
        }
    }

    void UpdatePosition()
    {
        Vector3 bestOffset = enableSmartPositioning ?
            FindBestPosition() : preferredOffset;

        targetWorldPosition = target.position + bestOffset;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(mainCam, targetWorldPosition);

        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            screenPos,
            parentCanvas.worldCamera,
            out canvasPos);

        // Clamp to screen bounds
        canvasPos = ClampToScreenBounds(canvasPos);

        transform.localPosition = canvasPos;
        originalPosition = canvasPos;
    }

    Vector3 FindBestPosition()
    {
        // Get character bounds
        Bounds characterBounds = GetCharacterBounds();

        // Test each fallback position
        foreach (Vector3 testOffset in fallbackPositions)
        {
            Vector3 testWorldPos = target.position + testOffset;

            // Check if position is clear of character
            if (IsPositionClearOfCharacter(testWorldPos, characterBounds))
            {
                // Check if position is visible on screen
                if (IsPositionVisibleOnScreen(testWorldPos))
                {
                    return testOffset;
                }
            }
        }

        // Return default if nothing works
        return preferredOffset;
    }

    Bounds GetCharacterBounds()
    {
        if (targetRenderer != null)
        {
            return targetRenderer.bounds;
        }
        else if (targetCollider != null)
        {
            return targetCollider.bounds;
        }
        else
        {
            // Create approximate bounds
            return new Bounds(target.position, Vector3.one * characterBoundsCheckRadius);
        }
    }

    bool IsPositionClearOfCharacter(Vector3 worldPosition, Bounds characterBounds)
    {
        // Check distance from character bounds
        float distance = Vector3.Distance(worldPosition, characterBounds.center);
        return distance >= (characterBounds.size.magnitude * 0.7f + minDistanceFromCharacter);
    }

    bool IsPositionVisibleOnScreen(Vector3 worldPosition)
    {
        Vector3 screenPoint = mainCam.WorldToScreenPoint(worldPosition);

        // Check if within screen bounds with some margin
        return screenPoint.x > 50 && screenPoint.x < Screen.width - 50 &&
               screenPoint.y > 50 && screenPoint.y < Screen.height - 50 &&
               screenPoint.z > 0;
    }

    Vector2 ClampToScreenBounds(Vector2 canvasPos)
    {
        if (bubbleBackground == null) return canvasPos;

        RectTransform canvasRect = parentCanvas.transform as RectTransform;
        Vector2 bubbleSize = bubbleBackground.sizeDelta;

        // Get canvas bounds
        Vector2 canvasSize = canvasRect.sizeDelta;
        Vector2 halfCanvas = canvasSize * 0.5f;
        Vector2 halfBubble = bubbleSize * 0.5f;

        // Clamp position
        canvasPos.x = Mathf.Clamp(canvasPos.x, -halfCanvas.x + halfBubble.x + 20,
                                  halfCanvas.x - halfBubble.x - 20);
        canvasPos.y = Mathf.Clamp(canvasPos.y, -halfCanvas.y + halfBubble.y + 20,
                                  halfCanvas.y - halfBubble.y - 20);

        return canvasPos;
    }

    void ApplyShakeEffect()
    {
        Vector3 shakeOffset = new Vector3(
            Random.Range(-shakeIntensity, shakeIntensity),
            Random.Range(-shakeIntensity, shakeIntensity),
            0
        );

        transform.localPosition = originalPosition + shakeOffset;
    }

    void ApplyBounceEffect()
    {
        bounceTimer += Time.deltaTime * bounceSpeed;
        float bounceOffset = Mathf.Sin(bounceTimer) * bounceHeight;

        Vector3 bouncePosition = originalPosition + new Vector3(0, bounceOffset, 0);
        transform.localPosition = bouncePosition;
    }

    private string currentMessage = "";

    public void Show(string message)
    {
        currentMessage = message; // Store the current message
        gameObject.SetActive(true);

        if (enableAdaptiveSizing)
        {
            AdaptBubbleSize(message);
        }

        StartCoroutine(AppearAndType(message));
    }

    void AdaptBubbleSize(string message)
    {
        if (bubbleBackground == null || dialogText == null) return;

        // Calculate preferred text size
        dialogText.text = message;
        dialogText.ForceMeshUpdate();

        Vector2 textSize = dialogText.textBounds.size;

        // Add padding
        Vector2 bubbleSize = new Vector2(
            textSize.x + paddingX,
            textSize.y + paddingY
        );

        // Clamp to min/max sizes
        bubbleSize.x = Mathf.Clamp(bubbleSize.x, minBubbleSize.x, maxBubbleSize.x);
        bubbleSize.y = Mathf.Clamp(bubbleSize.y, minBubbleSize.y, maxBubbleSize.y);

        bubbleBackground.sizeDelta = bubbleSize;

        // Clear text for typewriter effect
        dialogText.text = "";
    }

    IEnumerator AppearAndType(string message)
    {
        // Appear animation
        float elapsed = 0f;
        while (elapsed < appearDuration)
        {
            float progress = elapsed / appearDuration;
            float scaleValue = appearCurve.Evaluate(progress);
            transform.localScale = Vector3.one * scaleValue;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one;

        // Start typewriter effect
        typewriterCoroutine = StartCoroutine(TypewriterEffect(message));
    }

    IEnumerator TypewriterEffect(string message)
    {
        dialogText.text = "";
        isTypingComplete = false;
        bounceTimer = 0f;

        for (int i = 0; i < message.Length; i++)
        {
            dialogText.text += message[i];
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTypingComplete = true;
        enableShake = false;
    }

    // Versi SETELAH perbaikan
    public void Hide()
    {
        // PERIKSA dan KOSONGKAN teks sebelum memulai animasi hide
        if (dialogText != null)
        {
            dialogText.text = "";
        }

        // Jalankan animasi seperti biasa
        StartCoroutine(HideAnimation());
    }

    IEnumerator HideAnimation()
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < appearDuration * 0.5f)
        {
            float progress = elapsed / (appearDuration * 0.5f);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }

    public void CompleteTyping()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        // Set full text immediately
        if (dialogText != null && !string.IsNullOrEmpty(currentMessage))
        {
            dialogText.text = currentMessage;
        }

        isTypingComplete = true;
        enableShake = false;
    }

    // Public methods for external control
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            targetCollider = target.GetComponent<Collider>();
            targetRenderer = target.GetComponent<Renderer>();
        }
    }

    public void SetSmartPositioning(bool enabled)
    {
        enableSmartPositioning = enabled;
    }

    public void AddCustomFallbackPosition(Vector3 position)
    {
        List<Vector3> positions = new List<Vector3>(fallbackPositions);
        positions.Add(position);
        fallbackPositions = positions.ToArray();
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // Draw character bounds
        Bounds bounds = GetCharacterBounds();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        // Draw fallback positions
        Gizmos.color = Color.green;
        foreach (Vector3 offset in fallbackPositions)
        {
            Vector3 pos = target.position + offset;
            Gizmos.DrawWireSphere(pos, 0.2f);
        }

        // Draw current target position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetWorldPosition, 0.3f);
    }
}