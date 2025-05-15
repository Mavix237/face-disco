using UnityEngine;
using UnityEngine.SceneManagement;

public class Control : MonoBehaviour
{
    public void ToDIY()
    {
        SceneManager.LoadScene("DIY"); 
    }
    public void ToMusic()
    {
        SceneManager.LoadScene("Music"); 
    }
    public void Back()
    {
        SceneManager.LoadScene("Onboarding"); 
    }
    
}