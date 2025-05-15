using UnityEngine;

public class LookAtCamera : MonoBehaviour 
{
    public bool flipFacing = true;
    private Camera mainCamera;
    
    void Start() 
    {
        mainCamera = Camera.main;
        if (mainCamera == null) 
        {
            Debug.LogWarning("No main camera found for LookAtCamera component");
        }
    }
    
    void LateUpdate() 
    {
        if (mainCamera != null) 
        {
            // Make the object face the camera
            transform.LookAt(mainCamera.transform.position);
            
            // Optionally flip 180 degrees to face camera correctly (for quads)
            if (flipFacing) 
            {
                transform.Rotate(0, 180, 0);
            }
        }
    }
}