using UnityEngine;
using System.Collections.Generic;

public class PercussionBeatSource : BeatSource
{
     [Header("Percussion Settings")]
    public bool percussionMode = true;
    
    [Header("Timing Adjustment")]
    [Range(-0.2f, 0.2f)]
    public float audioOffsetTime = -0.05f; // Negative value makes sound play earlier
    
    [Header("Advanced Performance")]
    public bool usePreBufferedAudio = true;
    public float cooldownTimeOverride = 0.05f; // Super responsive cooldown (faster than BeatSource)
    
    // Reference to AudioPreBuffer
    private AudioPreBuffer audioPreBuffer;
    
    void Awake()
    {
        // Find or add AudioPreBuffer
        audioPreBuffer = FindAnyObjectByType<AudioPreBuffer>();
        if (audioPreBuffer == null && usePreBufferedAudio)
        {
            GameObject preBufferObj = new GameObject("AudioPreBuffer");
            audioPreBuffer = preBufferObj.AddComponent<AudioPreBuffer>();
            
            // Add our clips to be pre-buffered
            List<AudioClip> clipsToBuffer = new List<AudioClip>();
            if (smileAudioClip != null) clipsToBuffer.Add(smileAudioClip);
            if (jawOpenAudioClip != null) clipsToBuffer.Add(jawOpenAudioClip);
            if (browRaiseAudioClip != null) clipsToBuffer.Add(browRaiseAudioClip);
            
            audioPreBuffer.audioClips = clipsToBuffer.ToArray();
            audioPreBuffer.InitializeAudioPools();
        }
        
        Debug.Log("OptimizedPercussionBeatSource initialized");
    }
    
    void OnEnable()
    {
        // Override the base cooldown time for more responsiveness
        cooldownTime = cooldownTimeOverride;
    }
    
    protected override void PlaySmileClip()
    {
        if (smileAudioClip == null)
        {
            Debug.LogWarning("Smile audio clip is null");
            return;
        }
        
        // Use pre-buffered audio if available for maximum performance
        if (usePreBufferedAudio && audioPreBuffer != null)
        {
            if (audioOffsetTime != 0)
            {
                double scheduledTime = AudioSettings.dspTime + audioOffsetTime;
                audioPreBuffer.PlaySoundScheduled(smileAudioClip, scheduledTime);
            }
            else
            {
                audioPreBuffer.PlaySound(smileAudioClip);
            }
        }
        else
        {
            // Fallback to normal audio playback
            smileAudioSource.clip = smileAudioClip;
            
            if (audioOffsetTime != 0)
            {
                double scheduledTime = AudioSettings.dspTime + audioOffsetTime;
                smileAudioSource.PlayScheduled(scheduledTime);
            }
            else
            {
                smileAudioSource.Play();
            }
        }
        
        // Log and trigger visual effect - do this after audio is scheduled for better performance
        Debug.Log("Percussion: Smile percussion played");
        
        if (visualFeedback != null)
        {
            visualFeedback.ShowSmileEffect();
        }
    }
    
    protected override void PlayJawOpenClip()
    {
        if (jawOpenAudioClip == null)
        {
            Debug.LogWarning("Jaw open audio clip is null");
            return;
        }
        
        // Use pre-buffered audio if available for maximum performance
        if (usePreBufferedAudio && audioPreBuffer != null)
        {
            if (audioOffsetTime != 0)
            {
                double scheduledTime = AudioSettings.dspTime + audioOffsetTime;
                audioPreBuffer.PlaySoundScheduled(jawOpenAudioClip, scheduledTime);
            }
            else
            {
                audioPreBuffer.PlaySound(jawOpenAudioClip);
            }
        }
        else
        {
            // Fallback to normal audio playback
            jawOpenAudioSource.clip = jawOpenAudioClip;
            
            if (audioOffsetTime != 0)
            {
                double scheduledTime = AudioSettings.dspTime + audioOffsetTime;
                jawOpenAudioSource.PlayScheduled(scheduledTime);
            }
            else
            {
                jawOpenAudioSource.Play();
            }
        }
        
        Debug.Log("Percussion: Jaw open percussion played");
        
        if (visualFeedback != null)
        {
            visualFeedback.ShowJawOpenEffect();
        }
    }

    protected override void PlayBrowRaiseClip()
    {
        if (browRaiseAudioClip == null)
        {
            Debug.LogWarning("Brow raise audio clip is null");
            return;
        }
        
        // Use pre-buffered audio if available for maximum performance
        if (usePreBufferedAudio && audioPreBuffer != null)
        {
            if (audioOffsetTime != 0)
            {
                double scheduledTime = AudioSettings.dspTime + audioOffsetTime;
                audioPreBuffer.PlaySoundScheduled(browRaiseAudioClip, scheduledTime);
            }
            else
            {
                audioPreBuffer.PlaySound(browRaiseAudioClip);
            }
        }
        else
        {
            // Fallback to normal audio playback
            browRaiseAudioSource.clip = browRaiseAudioClip;
            
            if (audioOffsetTime != 0)
            {
                double scheduledTime = AudioSettings.dspTime + audioOffsetTime;
                browRaiseAudioSource.PlayScheduled(scheduledTime);
            }
            else
            {
                browRaiseAudioSource.Play();
            }
        }
        
        Debug.Log("Percussion: Brow raise percussion played");
        
        if (visualFeedback != null)
        {
            visualFeedback.ShowBrowRaiseEffect();
        }
    }
    
    // Override eye blink to do nothing in percussion mode
    protected override void PlayEyeBlinkClip(int noteIndex)
    {
        // Do nothing in percussion mode
    }
}