using UnityEngine;
using System.Collections.Generic;

public class BeatSource : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip smileAudioClip;
    public AudioClip jawOpenAudioClip;
    public AudioClip browRaiseAudioClip;
    
    [Header("Eye Blink Note Sequence")]
    public AudioClip[] eyeBlinkNotes; // Array of different pitched notes
    public bool randomizeEyeBlinkNotes = false; // Whether to play notes randomly or sequentially
    
    [Header("Settings")]
    
    [Header("Settings")]
    [Range(0, 1)]
    public float smileThreshold = 0.7f;
    public float cooldownTime = 0f;
    
    // Audio sources
    protected AudioSource smileAudioSource;
    protected AudioSource eyeBlinkAudioSource;
    protected AudioSource jawOpenAudioSource;
    protected AudioSource browRaiseAudioSource;

    protected float smileCooldown = 0f;
    protected float eyeBlinkCooldown = 0f;
    protected float jawOpenCooldown = 0f;
    protected float browRaiseCooldown = 0f;
    
    // Track the current note index for sequential playing
    protected int currentNoteIndex = 0;
    
    // Reference to visual feedback manager
    public VisualFeedback visualFeedback;
    
    void Start()
    {
        // Add debug logging
        Debug.Log("BeatSource Start method called");
                
        smileAudioSource = CreateAudioSource("SmileAudio");
        eyeBlinkAudioSource = CreateAudioSource("EyeBlinkAudio");
        jawOpenAudioSource = CreateAudioSource("JawOpenAudio");
        browRaiseAudioSource = CreateAudioSource("BrowRaiseAudio");
        
        // Get reference to the VisualFeedback component
        visualFeedback = FindAnyObjectByType<VisualFeedback>();
        if (visualFeedback == null)
        {
            Debug.LogWarning("VisualFeedback component not found in scene. Visual effects will not be displayed.");
        }
        else
        {
            Debug.Log("BeatSource: Found VisualFeedback");
        }
    }
    
    protected AudioSource CreateAudioSource(string name)
    {
        GameObject audioObj = new GameObject(name);
        audioObj.transform.parent = this.transform;
        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        
        // Force use of speaker on mobile devices
        #if UNITY_ANDROID || UNITY_IOS
        source.ignoreListenerPause = true;
        //source.ignoreListenerVolume = true;
        source.spatialBlend = 0f; // Force 2D audio (non-spatial)
        source.priority = 0; // Highest priority
        source.volume = 1.0f; // Full volume
        #endif
        
        return source;
    }
    
    void Update()
    {
        // Update all cooldowns
        smileCooldown -= Time.deltaTime;
        eyeBlinkCooldown -= Time.deltaTime;
        jawOpenCooldown -= Time.deltaTime;
        browRaiseCooldown -= Time.deltaTime;
    }
    
    // Method for triggering smile audio
    public void TriggerSmileAudio(float smileValue)
    {
        if (smileValue >= smileThreshold && smileCooldown <= 0f)
        {
            PlaySmileClip();
            smileCooldown = cooldownTime;
        }
    }
    
    // Method for triggering eye blink audio (with note sequence)
    public void TriggerEyeBlinkAudio(bool isBlinking)
    {
        if (isBlinking && eyeBlinkCooldown <= 0f)
        {
            // Debug log for troubleshooting
            Debug.Log("Eye blink detected, triggering audio");
            
            // Determine which note to play
            int noteIndex;
            if (randomizeEyeBlinkNotes)
            {
                // Play a random note
                noteIndex = Random.Range(0, eyeBlinkNotes.Length);
            }
            else
            {
                // Play the next note in sequence
                noteIndex = currentNoteIndex;
                // Advance to next note for next time
                currentNoteIndex = (currentNoteIndex + 1) % eyeBlinkNotes.Length;
            }
            
            PlayEyeBlinkClip(noteIndex);
            eyeBlinkCooldown = cooldownTime;
        }
    }
    
    // Method for triggering jaw open audio
    public void TriggerJawOpenAudio(bool isOpen)
    {
        if (isOpen && jawOpenCooldown <= 0f)
        {
            PlayJawOpenClip();
            jawOpenCooldown = cooldownTime;
        }
    }

    public void TriggerBrowRaiseAudio(bool isRaising)
    {
        if (isRaising && browRaiseCooldown <= 0f)
        {
            PlayBrowRaiseClip();
            browRaiseCooldown = cooldownTime;
        }
    }
    
    // Individual play methods for each feature
    protected virtual void PlaySmileClip()
    {
        if (smileAudioClip == null) 
        {
            Debug.LogWarning("Smile audio clip is null");
            return;
        }
        
        smileAudioSource.clip = smileAudioClip;
        smileAudioSource.Play();
        Debug.Log("Smile audio played");
        
        // Trigger visual effect
        if (visualFeedback != null)
        {
            visualFeedback.ShowSmileEffect();
        }
    }
    
    // Play the eye blink note with index
    protected virtual void PlayEyeBlinkClip(int noteIndex)
    {
        // Verify clip array is valid
        if (eyeBlinkNotes == null || eyeBlinkNotes.Length == 0)
        {
            Debug.LogWarning("Eye blink notes array is null or empty");
            return;
        }
        
        if (noteIndex >= eyeBlinkNotes.Length)
        {
            Debug.LogWarning($"Note index {noteIndex} is out of range for eye blink notes array (length: {eyeBlinkNotes.Length})");
            return;
        }
        
        eyeBlinkAudioSource.clip = eyeBlinkNotes[noteIndex];
        eyeBlinkAudioSource.Play();
        Debug.Log($"Eye blink audio played (note index: {noteIndex})");
        
        // Trigger visual effect - pass the note index to maintain the connection
        if (visualFeedback != null)
        {
            visualFeedback.ShowEyeBlinkEffect(noteIndex);
        }
    }
    
    protected virtual void PlayJawOpenClip()
    {
        if (jawOpenAudioClip == null)
        {
            Debug.LogWarning("Jaw open audio clip is null");
            return;
        }
        
        jawOpenAudioSource.clip = jawOpenAudioClip;
        jawOpenAudioSource.Play();
        Debug.Log("Jaw open audio played");
        
        // Trigger visual effect
        if (visualFeedback != null)
        {
            visualFeedback.ShowJawOpenEffect();
        }
    }

    protected virtual void PlayBrowRaiseClip()
    {
        if (browRaiseAudioClip == null)
        {
            Debug.LogWarning("Brow raise audio clip is null");
            return;
        }
        
        browRaiseAudioSource.clip = browRaiseAudioClip;
        browRaiseAudioSource.Play();
        Debug.Log("Brow raise audio played");
        
        // Trigger visual effect
        if (visualFeedback != null)
        {
            visualFeedback.ShowBrowRaiseEffect();
        }
    }
    
    // Public method to get the current number of eye blink notes
    public int GetEyeBlinkNoteCount()
    {
        return eyeBlinkNotes?.Length ?? 0;
    }
}