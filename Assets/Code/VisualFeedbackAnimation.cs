using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VisualEffectAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float animationDuration = 0.4f;
    public float moveDistance = 20f;
    public AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0, 0, 4, 4), new Keyframe(1, 1, 0, 0));
    public AnimationCurve moveCurve = new AnimationCurve(new Keyframe(0, 0, 4, 4), new Keyframe(1, 1, 0, 0));
    
    [Header("Scale Animation")]
    public bool animateScale = true;
    public float startScale = 0.7f;
    public float endScale = 2f;
    public AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0, 0, 4, 4), new Keyframe(1, 1, 0, 0));
    
    [Header("Rotation Animation")]
    public bool animateRotation = false;
    public float rotationAmount = 10f;
    public AnimationCurve rotationCurve = new AnimationCurve(new Keyframe(0, 0, 4, 4), new Keyframe(1, 1, 0, 0));
    
    [Header("Color Options")]
    public bool usePulseColor = false;
    public Color startColor = Color.white;
    public Color peakColor = new Color(1f, 1f, 1f, 1f);
    public Color endColor = new Color(1f, 1f, 1f, 0f);
    
    // Private references to components
    private RectTransform rectTransform;
    private Image image;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    
    // Track if this object is being animated
    private bool isAnimating = false;
    private PooledObject pooledObject;
    
    void Awake()
    {
        // Get necessary components
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        pooledObject = GetComponent<PooledObject>();
        
        // If no CanvasGroup, add one
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    void OnEnable()
    {
        // Start animations when object is enabled
        if (!isAnimating)
        {
            // Store starting values
            if (rectTransform != null)
            {
                startPosition = rectTransform.anchoredPosition;
                originalScale = rectTransform.localScale;
                originalRotation = rectTransform.localRotation;
            }
            
            // Start animations
            StartCoroutine(AnimateEffect());
        }
    }
    
    IEnumerator AnimateEffect()
    {
        isAnimating = true;
        
        float startTime = Time.time;
        float elapsedTime = 0f;
        
        // Set initial state
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        if (image != null && usePulseColor)
        {
            image.color = startColor;
        }
        
        if (animateScale && rectTransform != null)
        {
            rectTransform.localScale = originalScale * startScale;
        }
        
        // Animation loop
        while (elapsedTime < animationDuration)
        {
            elapsedTime = Time.time - startTime;
            float normalizedTime = elapsedTime / animationDuration;
            
            // Fade animation
            if (canvasGroup != null)
            {
                // First half fade in, second half fade out
                float fadeValue;
                if (normalizedTime < 0.5f)
                {
                    // Normalize to 0-1 range for first half
                    float firstHalfNormalized = normalizedTime * 2f;
                    fadeValue = fadeCurve.Evaluate(firstHalfNormalized);
                }
                else
                {
                    // Normalize to 0-1 range for second half (1 becomes 0, 0.5 becomes 1)
                    float secondHalfNormalized = (1f - normalizedTime) * 2f;
                    fadeValue = fadeCurve.Evaluate(secondHalfNormalized);
                }
                canvasGroup.alpha = fadeValue;
            }
            
            // Move upward animation
            if (rectTransform != null)
            {
                float moveProgress = moveCurve.Evaluate(normalizedTime);
                Vector2 newPosition = startPosition + Vector2.up * moveDistance * moveProgress;
                rectTransform.anchoredPosition = newPosition;
            }
            
            // Scale animation
            if (animateScale && rectTransform != null)
            {
                float scaleProgress = scaleCurve.Evaluate(normalizedTime);
                float currentScale = Mathf.Lerp(startScale, endScale, scaleProgress);
                rectTransform.localScale = originalScale * currentScale;
            }
            
            // Rotation animation
            if (animateRotation && rectTransform != null)
            {
                float rotationProgress = rotationCurve.Evaluate(normalizedTime);
                float currentRotation = rotationAmount * rotationProgress;
                rectTransform.localRotation = originalRotation * Quaternion.Euler(0, 0, currentRotation);
            }
            
            // Color pulse animation
            if (image != null && usePulseColor)
            {
                Color currentColor;
                if (normalizedTime < 0.5f)
                {
                    float colorProgress = normalizedTime * 2f;
                    currentColor = Color.Lerp(startColor, peakColor, colorProgress);
                }
                else
                {
                    float colorProgress = (normalizedTime - 0.5f) * 2f;
                    currentColor = Color.Lerp(peakColor, endColor, colorProgress);
                }
                image.color = currentColor;
            }
            
            yield return null;
        }
        
        // Ensure final state
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        isAnimating = false;
        
        // Return to pool if pooled object
        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
        }
        else
        {
            // Destroy if not pooled
            Destroy(gameObject);
        }
    }
}