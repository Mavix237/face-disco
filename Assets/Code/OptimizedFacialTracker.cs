using UnityEngine;
using UnityEngine.XR.ARKit;
using Unity.Collections;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class OptimizedFacialTracker : MonoBehaviour
{
    [Header("References")]
    public MonoBehaviour beatSourceComponent; // BeatSource or OptimizedPercussionBeatSource
    public UIManager uiManager;
    
    [Header("Detection Settings")]
    [Range(0.1f, 1.0f)]
    public float smileThreshold = 0.5f;  // Lower = more sensitive
    [Range(0.1f, 1.0f)]
    public float eyeBlinkThreshold = 0.5f;
    [Range(0.1f, 1.0f)]
    public float jawOpenThreshold = 0.4f;
    [Range(0.1f, 1.0f)]
    public float browRaiseThreshold = 0.4f;
    
    [Header("Performance")]
    public bool skipEyeBlinkDetection = false; // Skip in percussion mode
    
    // Private tracking variables
    private ARKitFaceSubsystem faceSubsystem;
    private ARFace face;
    
    // Cache reflection methods for performance
    private System.Reflection.MethodInfo triggerSmileMethod;
    private System.Reflection.MethodInfo triggerJawOpenMethod;
    private System.Reflection.MethodInfo triggerBrowRaiseMethod;
    private System.Reflection.MethodInfo triggerEyeBlinkMethod;
    
    // Cached state to avoid unnecessary method calls
    private bool wasSmiling = false;
    private bool wasJawOpen = false;
    private bool wasBrowRaised = false;
    private bool wasBlinking = false;
    
    // Optimized blend shape lookup
    private Dictionary<ARKitBlendShapeLocation, int> blendShapeIndices = new Dictionary<ARKitBlendShapeLocation, int>();
    private bool indicesInitialized = false;
    
    void Start()
    {
        // Get face components
        face = GetComponent<ARFace>();
        ARFaceManager faceManager = FindAnyObjectByType<ARFaceManager>();
        if (faceManager != null)
        {
            faceSubsystem = (ARKitFaceSubsystem)faceManager.subsystem;
        }
        
        // Find UI Manager if not assigned
        if (uiManager == null)
        {
            uiManager = FindAnyObjectByType<UIManager>();
        }
        
        // Find beat source if not assigned
        if (beatSourceComponent == null)
        {
            beatSourceComponent = FindAnyObjectByType<PercussionBeatSource>();
            if (beatSourceComponent == null)
            {
                beatSourceComponent = FindAnyObjectByType<BeatSource>();
            }
        }
        
        // Cache method info for better performance
        if (beatSourceComponent != null)
        {
            System.Type beatSourceType = beatSourceComponent.GetType();
            triggerSmileMethod = beatSourceType.GetMethod("TriggerSmileAudio");
            triggerJawOpenMethod = beatSourceType.GetMethod("TriggerJawOpenAudio");
            triggerBrowRaiseMethod = beatSourceType.GetMethod("TriggerBrowRaiseAudio");
            triggerEyeBlinkMethod = beatSourceType.GetMethod("TriggerEyeBlinkAudio");
        }
        
        Debug.Log("OptimizedFacialTracker initialized");
    }
    
    void Update()
    {
        if (faceSubsystem == null || face == null || beatSourceComponent == null) return;
        
        try
        {
            using (var blendShapes = faceSubsystem.GetBlendShapeCoefficients(face.trackableId, Allocator.Temp))
            {
                // Initialize blend shape indices for faster lookup on first update
                if (!indicesInitialized && blendShapes.Length > 0)
                {
                    for (int i = 0; i < blendShapes.Length; i++)
                    {
                        blendShapeIndices[blendShapes[i].blendShapeLocation] = i;
                    }
                    indicesInitialized = true;
                }
                
                // Process only the specific blend shapes we need using optimized lookup
                float smileMuch = 0;
                bool isJawOpen = false;
                bool isBrowRaise = false;
                bool isBlinking = false;
                
                // Smile detection (optimized)
                float leftSmile = GetBlendShapeValue(blendShapes, ARKitBlendShapeLocation.MouthSmileLeft);
                float rightSmile = GetBlendShapeValue(blendShapes, ARKitBlendShapeLocation.MouthSmileRight);
                float leftFrown = GetBlendShapeValue(blendShapes, ARKitBlendShapeLocation.MouthFrownLeft);
                float rightFrown = GetBlendShapeValue(blendShapes, ARKitBlendShapeLocation.MouthFrownRight);
                
                smileMuch = (leftSmile + rightSmile) - (leftFrown + rightFrown);
                bool isSmiling = smileMuch >= smileThreshold;
                
                // Only process the remaining expressions if needed
                isJawOpen = GetBlendShapeValue(blendShapes, ARKitBlendShapeLocation.JawOpen) >= jawOpenThreshold;
                isBrowRaise = GetBlendShapeValue(blendShapes, ARKitBlendShapeLocation.BrowInnerUp) >= browRaiseThreshold;
                
                if (!skipEyeBlinkDetection)
                {
                    float leftBlink = GetBlendShapeValue(blendShapes, ARKitBlendShapeLocation.EyeBlinkLeft);
                    float rightBlink = GetBlendShapeValue(blendShapes, ARKitBlendShapeLocation.EyeBlinkRight);
                    isBlinking = (leftBlink + rightBlink) / 2 >= eyeBlinkThreshold;
                }
                
                // Only trigger methods when the state changes from false to true
                // This reduces method calls and improves performance
                if (isSmiling != wasSmiling)
                {
                    wasSmiling = isSmiling;
                    if (isSmiling && triggerSmileMethod != null)
                    {
                        triggerSmileMethod.Invoke(beatSourceComponent, new object[] { smileMuch });
                    }
                }
                
                if (isJawOpen != wasJawOpen)
                {
                    wasJawOpen = isJawOpen;
                    if (isJawOpen && triggerJawOpenMethod != null)
                    {
                        triggerJawOpenMethod.Invoke(beatSourceComponent, new object[] { isJawOpen });
                    }
                }
                
                if (isBrowRaise != wasBrowRaised)
                {
                    wasBrowRaised = isBrowRaise;
                    if (isBrowRaise && triggerBrowRaiseMethod != null)
                    {
                        triggerBrowRaiseMethod.Invoke(beatSourceComponent, new object[] { isBrowRaise });
                    }
                }
                
                if (!skipEyeBlinkDetection && isBlinking != wasBlinking)
                {
                    wasBlinking = isBlinking;
                    if (isBlinking && triggerEyeBlinkMethod != null)
                    {
                        triggerEyeBlinkMethod.Invoke(beatSourceComponent, new object[] { isBlinking });
                    }
                }
                
                // Update UI if available
                if (uiManager != null)
                {
                    uiManager.UpdateText($"Smile: {smileMuch:F2}\nJaw: {isJawOpen}\nBrow: {isBrowRaise}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in face tracking: {e.Message}");
        }
    }
    
    // Optimized blend shape value lookup
    private float GetBlendShapeValue(NativeArray<ARKitBlendShapeCoefficient> blendShapes, ARKitBlendShapeLocation location)
    {
        if (indicesInitialized && blendShapeIndices.TryGetValue(location, out int index) && index < blendShapes.Length)
        {
            return blendShapes[index].coefficient;
        }
        
        // Fallback to linear search if needed
        for (int i = 0; i < blendShapes.Length; i++)
        {
            if (blendShapes[i].blendShapeLocation == location)
            {
                return blendShapes[i].coefficient;
            }
        }
        
        return 0f;
    }
}