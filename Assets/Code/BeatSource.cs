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
    public bool useMusicalScale = true; // Whether to use a preset musical scale

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
    
    // Reference to visual feedback manager
    public VisualFeedback visualFeedback;
    
    // Eye blink note sequence
    protected int currentNoteIndex = 0;
    
    [Header("Musical Note Generation")]
    public bool generateNotes = false; // Set to true to auto-generate notes
    public AudioClip baseNoteClip; // The base note clip that will be pitched
    public NoteScale noteScale = NoteScale.Major;
    public enum NoteScale { Major, Minor, Pentatonic, Blues, Chromatic }
    [Range(0, 3)]
    public int octaveRange = 1; // How many octaves the notes will span
    [Range(-12, 12)]
    public int baseNoteOffset = 0; // Shift the entire scale up or down by semitones
    
    // Dictionary for musical scales (semitones from root)
    protected Dictionary<NoteScale, int[]> musicalScales = new Dictionary<NoteScale, int[]>()
    {
        { NoteScale.Major, new int[] { 0, 2, 4, 5, 7, 9, 11 } },
        { NoteScale.Minor, new int[] { 0, 2, 3, 5, 7, 8, 10 } },
        { NoteScale.Pentatonic, new int[] { 0, 2, 4, 7, 9 } },
        { NoteScale.Blues, new int[] { 0, 3, 5, 6, 7, 10 } },
        { NoteScale.Chromatic, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 } }
    };
    
    
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
        
        // Set up the note clips for eye blinks if auto-generation is enabled
        if (generateNotes && baseNoteClip != null)
        {
            GenerateNoteSequence();
        }
        
        Debug.Log("BeatSource configuration: " + eyeBlinkNotes.Length + " eye blink notes, randomize: " + randomizeEyeBlinkNotes);
    }
    
    protected AudioSource CreateAudioSource(string name)
    {
        GameObject audioObj = new GameObject(name);
        audioObj.transform.parent = this.transform;
        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        return source;
    }
    
    // Method to generate a scale of notes from a single base note by adjusting pitch
    protected void GenerateNoteSequence()
    {
        if (baseNoteClip == null)
        {
            Debug.LogError("Base note clip is not assigned. Cannot generate note sequence.");
            return;
        }
        
        // Get the scale intervals
        int[] scaleIntervals = musicalScales[noteScale];
        
        // Calculate how many notes we need based on the scale and octave range
        int totalNotes = scaleIntervals.Length * (octaveRange + 1);
        eyeBlinkNotes = new AudioClip[totalNotes];
        
        // Create a dummy object to hold our generated clips
        for (int i = 0; i < totalNotes; i++)
        {
            // For organizational purposes, we'll just reference the same clip multiple times
            // The actual pitch change will happen at playback time
            eyeBlinkNotes[i] = baseNoteClip;
        }
        
        Debug.Log($"Generated {eyeBlinkNotes.Length} notes in {noteScale} scale");
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
    
    // Method for triggering eye blink audio (now with note sequence)
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
    
    // Play the eye blink note - virtual to allow override in child classes
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
        
        // If we're using note generation with pitch shifting
        if (generateNotes && baseNoteClip != null)
        {
            // Calculate the pitch multiplier based on semitone offset
            // The formula for semitone to pitch is: 2^(semitones/12)
            int[] scaleIntervals = musicalScales[noteScale];
            int octave = noteIndex / scaleIntervals.Length;
            int scaleIndex = noteIndex % scaleIntervals.Length;
            int semitoneOffset = scaleIntervals[scaleIndex] + (octave * 12) + baseNoteOffset;
            
            float pitchMultiplier = Mathf.Pow(2f, semitoneOffset / 12f);
            eyeBlinkAudioSource.pitch = pitchMultiplier;
            eyeBlinkAudioSource.clip = baseNoteClip;
        }
        else
        {
            // Use the pre-made audio clips
            eyeBlinkAudioSource.clip = eyeBlinkNotes[noteIndex];
            eyeBlinkAudioSource.pitch = 1f; // Reset pitch to normal
        }
        
        eyeBlinkAudioSource.Play();
        Debug.Log($"Eye blink audio played - Note {noteIndex}");
        
        // Trigger visual effect with the note index
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