using UnityEngine;
using TMPro;
using System.Collections;

public class EnhancedDialogBubble : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 2f, 0);

    [Header("UI Components")]
    public TextMeshProUGUI dialogText;
    public Canvas parentCanvas;

    [Header("Animation")]
    public float typewriterSpeed = 0.05f;
    public AnimationCurve appearCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float appearDuration = 0.5f;

    [Header("Visual Effects")]
    public bool enableShake = false;
    public float shakeIntensity = 0.1f;

    private Camera mainCam;
    private Vector3 originalPosition;
    private Coroutine typewriterCoroutine;
    private bool isTypingComplete = false;

    void Start()
    {
        mainCam = Camera.main;
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();

        originalPosition = transform.localPosition;

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
    }

    void UpdatePosition()
    {
        Vector3 worldPos = target.position + offset;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(mainCam, worldPos);

        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            screenPos,
            parentCanvas.worldCamera,
            out canvasPos);

        transform.localPosition = canvasPos;
        originalPosition = canvasPos;
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

    public void Show(string message)
    {
        gameObject.SetActive(true);
        StartCoroutine(AppearAndType(message));
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

        for (int i = 0; i < message.Length; i++)
        {
            dialogText.text += message[i];
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTypingComplete = true;
        enableShake = false;
    }

    public void Hide()
    {
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
        isTypingComplete = true;
        enableShake = false;
    }
}

