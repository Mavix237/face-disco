using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro; // Include this if you're using TextMeshPro

[System.Serializable]
public class TutorialSlide
{
    public string slideName;        // Name for editor organization
    public GameObject slideObject;  // Parent object containing both image and text
    public Image slideImage;        // The tutorial image
    public Text slideText;          // Regular UI Text (use this OR the next field)
    public TextMeshProUGUI slideTMP; // TextMeshPro Text (if you're using TMP)
    
    [TextArea(3, 10)]
    public string description;      // The text to display (set in inspector)
    
    // Call this to update the text component with the description
    public void UpdateText()
    {
        if (slideText != null)
        {
            slideText.text = description;
        }
        else if (slideTMP != null)
        {
            slideTMP.text = description;
        }
    }
}

public class OnboardingManager : MonoBehaviour
{
    [Header("Tutorial Slides")]
    public List<TutorialSlide> tutorialSlides = new List<TutorialSlide>();
    
    [Header("Settings")]
    public bool showOnStart = true;
    public bool saveProgress = true;  // This setting will be ignored for showing, but kept for other functionality
    public string tutorialKey = "DIY_Tutorial_Shown";
    
    [Header("Animation")]
    public float fadeInTime = 0.3f;
    public float fadeOutTime = 0.2f;
    
    // Private variables
    private int currentSlideIndex = -1;
    private bool tutorialActive = false;
    
    void Start()
    {
        // Initialize all slides
        foreach (TutorialSlide slide in tutorialSlides)
        {
            // Update text from the description field
            slide.UpdateText();
            
            // Add CanvasGroup if needed
            if (slide.slideObject != null)
            {
                CanvasGroup canvasGroup = slide.slideObject.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = slide.slideObject.AddComponent<CanvasGroup>();
                }
                
                // Hide the slide
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                slide.slideObject.SetActive(false);
            }
        }
        
        // MODIFIED: Always show tutorial on start, regardless of previous views
        if (showOnStart)
        {
            ShowTutorial();
        }
    }
    
    void Update()
    {
        // Check for clicks or taps when tutorial is active
        if (tutorialActive && (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            NextSlide();
        }
    }
    
    // Show the tutorial from the beginning
    public void ShowTutorial()
    {
        if (tutorialSlides.Count == 0)
        {
            Debug.LogWarning("Tutorial has no slides assigned!");
            return;
        }
        
        tutorialActive = true;
        currentSlideIndex = -1;
        NextSlide();
    }
    
    // Go to the next slide, or end tutorial if on last slide
    public void NextSlide()
    {
        // Hide current slide if one is showing
        if (currentSlideIndex >= 0 && currentSlideIndex < tutorialSlides.Count)
        {
            TutorialSlide currentSlide = tutorialSlides[currentSlideIndex];
            if (currentSlide.slideObject != null)
            {
                StartCoroutine(FadeOut(currentSlide.slideObject));
            }
        }
        
        // Move to next slide
        currentSlideIndex++;
        
        // Check if we've reached the end
        if (currentSlideIndex >= tutorialSlides.Count)
        {
            EndTutorial();
            return;
        }
        
        // Show the next slide
        TutorialSlide nextSlide = tutorialSlides[currentSlideIndex];
        if (nextSlide.slideObject != null)
        {
            StartCoroutine(FadeIn(nextSlide.slideObject));
        }
    }
    
    // End the tutorial
    private void EndTutorial()
    {
        tutorialActive = false;
        
        if (saveProgress)
        {
            PlayerPrefs.SetInt(tutorialKey, 1);
            PlayerPrefs.Save();
        }
    }
    
    // Check if player has seen tutorial before
    private bool HasSeenTutorial()
    {
        return PlayerPrefs.GetInt(tutorialKey, 0) == 1;
    }
    
    // Fade in a slide
    private IEnumerator FadeIn(GameObject slide)
    {
        slide.SetActive(true);
        CanvasGroup canvasGroup = slide.GetComponent<CanvasGroup>();
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        float startTime = Time.time;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInTime)
        {
            elapsedTime = Time.time - startTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / fadeInTime);
            
            canvasGroup.alpha = normalizedTime;
            
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    // Fade out a slide
    private IEnumerator FadeOut(GameObject slide)
    {
        CanvasGroup canvasGroup = slide.GetComponent<CanvasGroup>();
        
        float startTime = Time.time;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutTime)
        {
            elapsedTime = Time.time - startTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / fadeOutTime);
            
            canvasGroup.alpha = 1f - normalizedTime;
            
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        slide.SetActive(false);
    }
    
    // For testing: reset tutorial to show again
    public void ResetTutorial()
    {
        if (saveProgress)
        {
            PlayerPrefs.DeleteKey(tutorialKey);
            PlayerPrefs.Save();
        }
    }
}