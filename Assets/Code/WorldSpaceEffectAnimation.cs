using UnityEngine;
using System.Collections;

public class WorldSpaceEffectAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float animationDuration = 0.7f;
    public float moveDistance = 0.05f;
    public AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0, 0, 4, 4), new Keyframe(1, 1, 0, 0));
    
    [Header("Scale Animation")]
    public bool animateScale = true;
    public float startScale = 0.8f;
    public float endScale = 1.2f;
    public AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0, 0, 4, 4), new Keyframe(1, 1, 0, 0));
    
    [Header("Color Options")]
    public bool usePulseColor = true;
    public Color startColor = new Color(1f, 1f, 1f, 0.2f);
    public Color peakColor = new Color(1f, 1f, 1f, 1f);
    public Color endColor = new Color(1f, 1f, 1f, 0f);
    
    // Private references
    private Transform quadTransform;
    private Vector3 startPosition;
    private Vector3 originalScale;
    private Material materialInstance;
    private bool usingMaterialInstance = false;
    private MeshRenderer meshRenderer;
    
    // Track animation state
    private bool isAnimating = false;
    private Coroutine animationCoroutine;
    
    void Awake()
    {
        // Get necessary components
        quadTransform = transform;
        meshRenderer = GetComponent<MeshRenderer>();
        
        // Create a material instance for animation
        if (meshRenderer != null && meshRenderer.material != null)
        {
            materialInstance = new Material(meshRenderer.material);
            meshRenderer.material = materialInstance;
            usingMaterialInstance = true;
        }
    }
    
    void OnEnable()
    {
        // Only start animation if not already running
        if (!isAnimating)
        {
            // Store initial values
            startPosition = quadTransform.position;
            originalScale = quadTransform.localScale;
            
            // Start animation
            animationCoroutine = StartCoroutine(AnimateEffect());
        }
    }
    
    void OnDisable()
    {
        // Stop animation if running
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        
        // Clean up material instance
        if (usingMaterialInstance && materialInstance != null)
        {
            if (Application.isPlaying)
            {
                Destroy(materialInstance);
            }
            else
            {
                DestroyImmediate(materialInstance);
            }
            usingMaterialInstance = false;
        }
    }
    
    IEnumerator AnimateEffect()
    {
        isAnimating = true;
        
        float startTime = Time.time;
        float elapsedTime = 0f;
        
        // Set initial state
        if (materialInstance != null && usePulseColor)
        {
            materialInstance.color = startColor;
        }
        
        if (animateScale)
        {
            quadTransform.localScale = originalScale * startScale;
        }
        
        // Animation loop
        while (elapsedTime < animationDuration)
        {
            elapsedTime = Time.time - startTime;
            float normalizedTime = elapsedTime / animationDuration;
            
            // Move upward animation
            float moveProgress = fadeCurve.Evaluate(normalizedTime);
            Vector3 newPosition = startPosition + Vector3.up * moveDistance * moveProgress;
            quadTransform.position = newPosition;
            
            // Scale animation
            if (animateScale)
            {
                float scaleProgress = scaleCurve.Evaluate(normalizedTime);
                float currentScale = Mathf.Lerp(startScale, endScale, scaleProgress);
                quadTransform.localScale = originalScale * currentScale;
            }
            
            // Color animation
            if (materialInstance != null && usePulseColor)
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
                materialInstance.color = currentColor;
            }
            
            yield return null;
        }
        
        // Ensure final state before destroying
        if (materialInstance != null)
        {
            materialInstance.color = endColor;
        }
        
        isAnimating = false;
        Destroy(gameObject);
    }
}