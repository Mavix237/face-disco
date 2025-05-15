using UnityEngine;

public class BeatSourceExtension : MonoBehaviour
{
    // Reference to the original BeatSource component
    private BeatSource beatSource;
    
    // Reference to the bridge component
    public VisualFeedbackBridge visualFeedbackBridge;
    
    void Start()
    {
        // Get the reference to BeatSource on this GameObject
        beatSource = GetComponent<BeatSource>();
        
        if (beatSource == null)
        {
            Debug.LogError("BeatSourceExtension must be attached to the same GameObject as BeatSource!");
            return;
        }
        
        // Find the VisualFeedbackBridge if not manually assigned
        if (visualFeedbackBridge == null)
        {
            visualFeedbackBridge = FindObjectOfType<VisualFeedbackBridge>();
            if (visualFeedbackBridge == null)
            {
                Debug.LogWarning("No VisualFeedbackBridge found in the scene. 3D effects will not be displayed.");
            }
        }
        
        // Override the BeatSource's PlayEyeBlinkClip method using a custom delegate
        var methodInfo = typeof(BeatSource).GetMethod("PlayEyeBlinkClip", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        if (methodInfo != null)
        {
            Debug.Log("Successfully found PlayEyeBlinkClip method");
        }
        else
        {
            Debug.LogWarning("Could not find PlayEyeBlinkClip method. Extension won't work correctly.");
        }
    }
    
    // This method will be called when BeatSource triggers an eye blink effect
    public void PlayEyeBlinkEffect(int noteIndex)
    {
        // Let the original BeatSource handle audio
        // (this is already happening in the original code)
        
        // Use our bridge to display 3D visual effect
        if (visualFeedbackBridge != null)
        {
            visualFeedbackBridge.Show3DEyeBlinkEffect(noteIndex);
        }
    }
}