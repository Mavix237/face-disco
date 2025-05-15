using UnityEngine;
using UnityEngine.XR.ARKit;
using Unity.Collections;
using UnityEngine.XR.ARFoundation;

public class FacialExpressionDetector : MonoBehaviour
{
    [Header("Expression References")]
    public BeatSource beatSource;
    public BackgroundMusicPlayer musicPlayer;
    public VisualFeedback visualFeedback;
    
    [Header("Sad Face Settings")]
    [Range(0, 1)]
    public float frownThreshold = 0.4f;       // How strong the frown must be
    public float mouthFrownWeight = 1.0f;     // Weight for mouth frown
    public float browDownWeight = 0.7f;       // Weight for lowered brows
    public float cooldownTime = 3.0f;         // Prevent retriggering too quickly
    
    [Header("Optional Feedback")]
    public AudioClip sadFaceSoundEffect;      // Optional sound when sad face detected
    public GameObject sadFaceVisualEffect;    // Optional visual when sad face detected
    
    // Private tracking variables
    private ARKitFaceSubsystem faceSubsystem;
    private ARFace face;
    private float lastTriggerTime = -10f;     // Time since last trigger (negative to allow immediate first trigger)
    private bool musicStarted = false;
    private AudioSource audioSource;
    
    void Start()
    {
        // Get face tracking references
        face = GetComponent<ARFace>();
        ARFaceManager faceManager = FindAnyObjectByType<ARFaceManager>();
        if (faceManager != null)
        {
            faceSubsystem = (ARKitFaceSubsystem)faceManager.subsystem;
        }
        
        // Create audio source if needed
        if (sadFaceSoundEffect != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = sadFaceSoundEffect;
            audioSource.playOnAwake = false;
        }
        
        // Debug logs
        Debug.Log("FacialExpressionDetector: Initialized");
        Debug.Log("Face reference: " + (face != null));
        Debug.Log("FaceSubsystem: " + (faceSubsystem != null));
        Debug.Log("BeatSource: " + (beatSource != null));
        Debug.Log("MusicPlayer: " + (musicPlayer != null));
    }
    
    void Update()
    {
        // Return if music already started
        if (musicStarted) return;
        
        // Check if we can trigger again
        if (Time.time - lastTriggerTime < cooldownTime) return;
        
        // Ensure we have all needed components
        if (faceSubsystem == null || face == null || musicPlayer == null) return;
        
        // Check for sad face expression
        try
        {
            using (var blendShapes = faceSubsystem.GetBlendShapeCoefficients(face.trackableId, Allocator.Temp))
            {
                // Track frown indicators
                float mouthFrownValue = 0;
                float browDownValue = 0;
                
                // Analyze blend shapes
                foreach (var featureCoefficient in blendShapes)
                {
                    // Mouth frown (main indicator)
                    if (featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.MouthFrownLeft)
                        mouthFrownValue += featureCoefficient.coefficient;
                    if (featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.MouthFrownRight)
                        mouthFrownValue += featureCoefficient.coefficient;
                        
                    // Brow lowered (secondary indicator)
                    if (featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.BrowDownLeft)
                        browDownValue += featureCoefficient.coefficient;
                    if (featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.BrowDownRight)
                        browDownValue += featureCoefficient.coefficient;
                }
                
                // Normalize values
                mouthFrownValue /= 2.0f; // Average of left and right
                browDownValue /= 2.0f;   // Average of left and right
                
                // Calculate overall sad face value
                float sadFaceValue = (mouthFrownValue * mouthFrownWeight + browDownValue * browDownWeight) 
                                    / (mouthFrownWeight + browDownWeight);
                
                // Debug log the value occasionally
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"Sad face value: {sadFaceValue:F2} (Threshold: {frownThreshold:F2})");
                }
                
                // Check if we've reached the threshold
                if (sadFaceValue >= frownThreshold)
                {
                    TriggerSadFace();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in face expression detection: {e.Message}");
        }
    }
    
    private void TriggerSadFace()
    {
        // Update trigger time
        lastTriggerTime = Time.time;
        
        // Start music if not already started
        if (!musicStarted && musicPlayer != null)
        {
            Debug.Log("🎵 SAD FACE DETECTED - STARTING CREEP");
            musicPlayer.StartMusic();
            musicStarted = true;
            
            // Play sound effect if available
            if (audioSource != null && sadFaceSoundEffect != null)
            {
                audioSource.Play();
            }
            
            // Show visual effect if available
            if (visualFeedback != null && sadFaceVisualEffect != null)
            {
                // Custom sad face visual effect could be shown here
                // visualFeedback.ShowSadFaceEffect();
            }
        }
    }
}