using UnityEngine;

public class VisualFeedbackBridge : MonoBehaviour
{
    [Header("3D Effect Prefabs")]
    public GameObject[] note3DPrefabs; // Your 3D Quad prefabs for each note
    public GameObject default3DPrefab; // Default 3D prefab to use if no note-specific one exists
    
    [Header("Animation Settings")]
    public float effectDuration = 0.7f;
    public float moveDistance = 0.05f;
    public bool animateScale = true;
    
    // Reference to main VisualFeedback component
    private VisualFeedback visualFeedback;
    
    void Start()
    {
        // Find the VisualFeedback component
        visualFeedback = GetComponent<VisualFeedback>();
        
        if (visualFeedback == null)
        {
            Debug.LogError("VisualFeedbackBridge needs to be on the same GameObject as VisualFeedback!");
            return;
        }
        
        // Hook into the VisualFeedback component
        // This is done by reflection to avoid modifying the original script
        var methodInfo = typeof(VisualFeedback).GetMethod("ShowEyeBlinkEffect", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        if (methodInfo != null)
        {
            Debug.Log("Successfully found ShowEyeBlinkEffect method");
        }
        else
        {
            Debug.LogWarning("Could not find ShowEyeBlinkEffect method. 3D effects may not work properly.");
        }
    }
    
    // This method is called by your BeatSource script
    public void Show3DEyeBlinkEffect(int noteIndex)
    {
        // Skip if no prefabs are assigned
        if (note3DPrefabs == null || note3DPrefabs.Length == 0)
        {
            Debug.LogWarning("No 3D prefabs assigned to VisualFeedbackBridge!");
            return;
        }
        
        // Select the appropriate prefab
        GameObject prefabToUse;
        if (noteIndex >= 0 && noteIndex < note3DPrefabs.Length && note3DPrefabs[noteIndex] != null)
        {
            prefabToUse = note3DPrefabs[noteIndex];
        }
        else if (default3DPrefab != null)
        {
            prefabToUse = default3DPrefab;
        }
        else
        {
            Debug.LogWarning("No valid 3D prefab found for note index: " + noteIndex);
            return;
        }
        
        // Get position from the VisualFeedback component
        Vector3 effectPosition = new Vector3(0, 0.2f, 0.3f); // Default position
        
        // Try to get the position from VisualFeedback
        System.Reflection.FieldInfo posField = typeof(VisualFeedback).GetField("eyeBlinkEffectPosition", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (posField != null)
        {
            effectPosition = (Vector3)posField.GetValue(visualFeedback);
        }
        
        // Get face transform
        Transform faceTransform = null;
        System.Reflection.FieldInfo faceField = typeof(VisualFeedback).GetField("faceTransform", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (faceField != null)
        {
            faceTransform = (Transform)faceField.GetValue(visualFeedback);
        }
        
        // Calculate world position
        Vector3 worldPos = faceTransform != null 
            ? faceTransform.TransformPoint(effectPosition) 
            : Camera.main.transform.TransformPoint(effectPosition);
        
        // Instantiate the 3D effect
        GameObject effectInstance = Instantiate(prefabToUse, worldPos, Quaternion.identity);
        
        // Add animation component
        WorldSpaceEffectAnimation animation = effectInstance.GetComponent<WorldSpaceEffectAnimation>();
        if (animation == null)
        {
            animation = effectInstance.AddComponent<WorldSpaceEffectAnimation>();
        }
        
        // Configure animation
        animation.animationDuration = effectDuration;
        animation.moveDistance = moveDistance;
        animation.animateScale = animateScale;
        
        // Add LookAtCamera component if needed
        LookAtCamera lookAt = effectInstance.GetComponent<LookAtCamera>();
        if (lookAt == null)
        {
            lookAt = effectInstance.AddComponent<LookAtCamera>();
            lookAt.flipFacing = true;
        }
    }
    
    // Helper method to create prefabs manually if needed
    public GameObject Create3DPrefab(Texture2D texture, Color color)
    {
        // Create a new GameObject with a quad
        GameObject prefab = new GameObject("Note3DPrefab");
        
        // Add mesh filter with quad mesh
        MeshFilter meshFilter = prefab.AddComponent<MeshFilter>();
        meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        
        // Add mesh renderer with transparent material
        MeshRenderer meshRenderer = prefab.AddComponent<MeshRenderer>();
        Material material = new Material(Shader.Find("UI/Unlit/Transparent"));
        material.mainTexture = texture;
        material.color = color;
        meshRenderer.material = material;
        
        // Add animation component
        prefab.AddComponent<WorldSpaceEffectAnimation>();
        
        // Add look at camera component
        LookAtCamera lookAt = prefab.AddComponent<LookAtCamera>();
        lookAt.flipFacing = true;
        
        return prefab;
    }
}