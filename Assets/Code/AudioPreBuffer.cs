using UnityEngine;
using System.Collections.Generic;

public class AudioPreBuffer : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip[] audioClips;  // Assign all audio clips used in your app
    
    [Header("Settings")]
    public int poolSize = 3;  // How many instances of each sound to pre-create
    public bool debugMode = true;
    
    // Dictionary to store pools of AudioSources for each clip
    private Dictionary<AudioClip, List<AudioSource>> audioSourcePools = new Dictionary<AudioClip, List<AudioSource>>();
    
    void Awake()
    {
        // Initialize audio pools as early as possible
        InitializeAudioPools();
    }
    
    public void InitializeAudioPools()
    {
        // For each audio clip
        foreach (AudioClip clip in audioClips)
        {
            if (clip == null) continue;
            
            // Create a new list for this clip
            List<AudioSource> sourcePool = new List<AudioSource>();
            
            // Create multiple AudioSources for this clip
            for (int i = 0; i < poolSize; i++)
            {
                // Create a child GameObject for this audio source
                GameObject audioSourceObj = new GameObject("PreBuffer_" + clip.name + "_" + i);
                audioSourceObj.transform.parent = transform;
                
                // Add and configure AudioSource
                AudioSource source = audioSourceObj.AddComponent<AudioSource>();
                source.clip = clip;
                source.playOnAwake = false;
                source.volume = 0f;  // Start with zero volume
                
                // Pre-buffer by playing once silently
                source.Play();
                source.Pause();
                source.time = 0f;
                source.volume = 1f;  // Reset volume
                
                // Add to pool
                sourcePool.Add(source);
            }
            
            // Store pool in dictionary
            audioSourcePools.Add(clip, sourcePool);
            
            if (debugMode)
            {
                Debug.Log($"Pre-buffered {poolSize} instances of {clip.name}");
            }
        }
    }
    
    // Play audio with minimal latency
    public void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        
        // Check if we have a pool for this clip
        if (audioSourcePools.TryGetValue(clip, out List<AudioSource> sourcePool))
        {
            // Find an available source
            foreach (AudioSource source in sourcePool)
            {
                if (!source.isPlaying)
                {
                    // Found an available source, play immediately
                    source.Play();
                    return;
                }
            }
            
            // If all sources are in use, use the first one (oldest)
            sourcePool[0].Stop();
            sourcePool[0].Play();
        }
        else
        {
            Debug.LogWarning($"Clip {clip.name} not pre-buffered!");
        }
    }
    
    // Play with a specific scheduling
    public void PlaySoundScheduled(AudioClip clip, double time)
    {
        if (clip == null) return;
        
        // Check if we have a pool for this clip
        if (audioSourcePools.TryGetValue(clip, out List<AudioSource> sourcePool))
        {
            // Find an available source
            foreach (AudioSource source in sourcePool)
            {
                if (!source.isPlaying)
                {
                    // Found an available source, schedule it
                    source.PlayScheduled(time);
                    return;
                }
            }
            
            // If all sources are in use, use the first one (oldest)
            sourcePool[0].Stop();
            sourcePool[0].PlayScheduled(time);
        }
        else
        {
            Debug.LogWarning($"Clip {clip.name} not pre-buffered!");
        }
    }
}