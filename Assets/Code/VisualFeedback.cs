using UnityEngine;
using System.Collections;
using UnityEngine.XR.ARFoundation;

public class VisualFeedback : MonoBehaviour
{
    [Header("Visual Effect Prefabs")]
    public GameObject smileEffectPrefab;
    public GameObject jawOpenEffectPrefab;
    public GameObject browRaiseEffectPrefab;
    
    [Header("Eye Blink Note Sequence Visuals")]
    public GameObject[] eyeBlinkNotePrefabs; // Array of different prefabs for each note
    public bool useNotePrefabs = true; // Set to true if you have prefabs for each note
    public GameObject defaultEyeBlinkPrefab; // Fallback if you don't have enough prefabs
    
    [Header("Effect Settings")]
    public float effectDuration = 0.6f;      // How long each visual effect stays on screen
    public Vector3 effectScale = new Vector3(1f, 1f, 1f);  // Scale of the visual effects
    public bool useWorldSpace = false;        // Whether to place effects in world space or screen space
    
    [Header("Note Customization")]
    public bool customizeNoteAppearance = true;
    public Gradient noteColorGradient;        // Colors based on note pitch
    public float minNoteScale = 0.8f;         // Scale for lowest note
    public float maxNoteScale = 1.5f;         // Scale for highest note
    
    [Header("Animation Settings")]
    public bool addAnimation = true;          // Whether to add animation components
    public float moveDistance = 50f;          // How far up the note moves
    public float animationDuration = 0.5f;    // How long the animation takes
    
    [Header("Positioning")]
    public Vector3 smileEffectPosition = new Vector3(0, -0.3f, 0.3f);      // Position relative to face
    public Vector3 eyeBlinkEffectPosition = new Vector3(0, 0.2f, 0.3f);    // Position relative to face
    public Vector3 jawOpenEffectPosition = new Vector3(0, -0.7f, 0.3f);    // Position relative to face
    public Vector3 browRaiseEffectPosition = new Vector3(0, 0.5f, 0.3f);   // Position relative to face
    
    [Header("Canvas Settings (if not using world space)")]
    public Canvas uiCanvas;
    public RectTransform smileEffectAnchor;
    public RectTransform eyeBlinkEffectAnchor;
    public RectTransform jawOpenEffectAnchor;
    public RectTransform browRaiseEffectAnchor;
    
    // Reference to BeatSource for note information
    private BeatSource beatSource;
    
    private Transform faceTransform;
    
    void Start()
    {
        // Get reference to the AR face transform
        ARFace face = FindAnyObjectByType<ARFace>();
        if (face != null)
        {
            faceTransform = face.transform;
            Debug.Log("VisualFeedback: Found ARFace");
        }
        else
        {
            Debug.LogWarning("ARFace not found. Visual effects may not position correctly.");
        }
        
        // Find BeatSource reference
        beatSource = FindAnyObjectByType<BeatSource>();
        if (beatSource != null)
        {
            Debug.Log("VisualFeedback: Found BeatSource");
        }
        else
        {
            Debug.LogWarning("BeatSource not found. Note sequence visualization may not work correctly.");
        }
        
        // Initialize color gradient if not set
        if (customizeNoteAppearance && noteColorGradient.colorKeys.Length == 0)
        {
            // Create a default blue to green gradient
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0].color = new Color(0.3f, 0.5f, 0.9f); // Blue-ish
            colorKeys[0].time = 0.0f;
            colorKeys[1].color = new Color(0.4f, 0.8f, 0.6f); // Teal-ish
            colorKeys[1].time = 0.5f;
            colorKeys[2].color = new Color(0.5f, 0.9f, 0.4f); // Green-ish
            colorKeys[2].time = 1.0f;
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0].alpha = 1.0f;
            alphaKeys[0].time = 0.0f;
            alphaKeys[1].alpha = 1.0f;
            alphaKeys[1].time = 1.0f;
            
            noteColorGradient.SetKeys(colorKeys, alphaKeys);
        }
    }
    
    // Public methods to be called when each audio trigger fires
    public void ShowSmileEffect()
    {
        CreateVisualEffect(smileEffectPrefab, smileEffectPosition, smileEffectAnchor);
    }
    
    // Show the eye blink effect - now with note index support
    public void ShowEyeBlinkEffect(int noteIndex = -1)
    {
        GameObject prefabToUse;
        
        // If using specific note prefabs and we have enough, use the appropriate one
        if (useNotePrefabs && eyeBlinkNotePrefabs != null && eyeBlinkNotePrefabs.Length > 0)
        {
            // If noteIndex is not specified, get the current one from BeatSource
            if (noteIndex < 0 && beatSource != null)
            {
                // Try to access the currentNoteIndex through reflection (since it's protected)
                System.Reflection.FieldInfo fieldInfo = 
                    typeof(BeatSource).GetField("currentNoteIndex", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.NonPublic);
                
                if (fieldInfo != null)
                {
                    noteIndex = (int)fieldInfo.GetValue(beatSource);
                }
            }
            
            // Use the note-specific prefab or fall back to default
            if (noteIndex >= 0 && noteIndex < eyeBlinkNotePrefabs.Length && eyeBlinkNotePrefabs[noteIndex] != null)
            {
                prefabToUse = eyeBlinkNotePrefabs[noteIndex];
            }
            else if (defaultEyeBlinkPrefab != null)
            {
                prefabToUse = defaultEyeBlinkPrefab;
            }
            else if (eyeBlinkNotePrefabs.Length > 0 && eyeBlinkNotePrefabs[0] != null)
            {
                // Fall back to first prefab if available
                prefabToUse = eyeBlinkNotePrefabs[0];
            }
            else
            {
                Debug.LogWarning("No valid eye blink prefab found. Cannot show effect.");
                return;
            }
        }
        else if (defaultEyeBlinkPrefab != null)
        {
            prefabToUse = defaultEyeBlinkPrefab;
        }
        else
        {
            Debug.LogWarning("No eye blink prefab assigned. Cannot show effect.");
            return;
        }
        
        // Get total number of notes for normalization
        int totalNotes = eyeBlinkNotePrefabs != null ? eyeBlinkNotePrefabs.Length : 1;
        
        // Create the visual effect with appropriate customization
        GameObject effect = CreateVisualEffect(prefabToUse, eyeBlinkEffectPosition, eyeBlinkEffectAnchor);
        
        // Customize for the specific note
        if (customizeNoteAppearance && effect != null && noteIndex >= 0)
        {
            CustomizeNoteVisual(effect, noteIndex, totalNotes);
        }
    }
    
    public void ShowJawOpenEffect()
    {
        CreateVisualEffect(jawOpenEffectPrefab, jawOpenEffectPosition, jawOpenEffectAnchor);
    }
    
    public void ShowBrowRaiseEffect()
    {
        CreateVisualEffect(browRaiseEffectPrefab, browRaiseEffectPosition, browRaiseEffectAnchor);
    }
    
    // Helper to create visual effects in either world or screen space
    private GameObject CreateVisualEffect(GameObject prefab, Vector3 position, RectTransform anchor)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Effect prefab is missing!");
            return null;
        }
        
        GameObject effectInstance;
        
        if (useWorldSpace)
        {
            // Create in world space relative to face
            Vector3 worldPos = faceTransform != null 
                ? faceTransform.TransformPoint(position) 
                : Camera.main.transform.TransformPoint(position);
                
            effectInstance = Instantiate(prefab, worldPos, Quaternion.identity);
            effectInstance.transform.localScale = effectScale;
            
            // Make effect face the camera
            effectInstance.transform.LookAt(Camera.main.transform);
            effectInstance.transform.Rotate(0, 180, 0); // Flip to face camera
            
            // Add animation component for world space
            if (addAnimation)
            {
                AddWorldSpaceAnimation(effectInstance);
            }
        }
        else
        {
            // Create in UI/screen space
            if (uiCanvas == null || anchor == null)
            {
                Debug.LogWarning("Canvas or anchor is missing for UI mode!");
                return null;
            }
            
            effectInstance = Instantiate(prefab, anchor);
            RectTransform rectTransform = effectInstance.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = effectScale;
            }
            
            // Add UI animation component
            if (addAnimation)
            {
                AddUIAnimation(effectInstance);
            }
        }
        
        // Destroy after duration if no animation component added
        if (!addAnimation)
        {
            Destroy(effectInstance, effectDuration);
        }
        
        return effectInstance;
    }
    
    // Helper to customize note visuals based on pitch
    private void CustomizeNoteVisual(GameObject noteEffect, int noteIndex, int totalNotes)
    {
        if (noteEffect == null || totalNotes <= 1) return;
        
        // Calculate normalized position in scale (0-1)
        float normalizedPitch = (float)noteIndex / (totalNotes - 1);
        
        // Get color from gradient
        Color noteColor = noteColorGradient.Evaluate(normalizedPitch);
        
        // Apply to Image or Sprite
        UnityEngine.UI.Image image = noteEffect.GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            image.color = noteColor;
        }
        
        SpriteRenderer spriteRenderer = noteEffect.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = noteColor;
        }
        
        // Scale based on pitch (higher notes = larger)
        float scaleMultiplier = Mathf.Lerp(minNoteScale, maxNoteScale, normalizedPitch);
        
        if (!useWorldSpace)
        {
            RectTransform rectTransform = noteEffect.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localScale = effectScale * scaleMultiplier;
            }
        }
        else
        {
            noteEffect.transform.localScale = effectScale * scaleMultiplier;
        }
        
        // If we have an animation component, update its parameters
        VisualEffectAnimation animation = noteEffect.GetComponent<VisualEffectAnimation>();
        if (animation != null)
        {
            // Customize animation based on pitch
            animation.moveDistance = 30f + normalizedPitch * 30f; // Higher notes move more
            animation.startScale = 0.6f + normalizedPitch * 0.4f; // Higher notes start bigger
            animation.endScale = 1.0f + normalizedPitch * 0.8f;   // Higher notes end bigger
            
            // Optional: higher notes can animate faster
            animation.animationDuration = Mathf.Lerp(0.6f, 0.4f, normalizedPitch);
            
            // Update colors
            animation.usePulseColor = true;
            animation.startColor = new Color(noteColor.r, noteColor.g, noteColor.b, 0.4f);
            animation.peakColor = noteColor;
            animation.endColor = new Color(noteColor.r, noteColor.g, noteColor.b, 0f);
        }
    }
    
    // Helper to add UI animation
    private void AddUIAnimation(GameObject effect)
    {
        VisualEffectAnimation animation = effect.AddComponent<VisualEffectAnimation>();
        animation.animationDuration = effectDuration;
        animation.moveDistance = moveDistance;
        animation.animateScale = true;
        animation.startScale = 0.7f;
        animation.endScale = 1.2f;
        animation.usePulseColor = false;  // Will be customized per note if needed
    }
    
    // Helper to add world space animation
    private void AddWorldSpaceAnimation(GameObject effect)
    {
        // Add your world space animation logic here
        // This might involve a different animation component for 3D objects
        Destroy(effect, effectDuration); // Default fallback
    }
}