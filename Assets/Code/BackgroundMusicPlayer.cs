using UnityEngine;
using System.Collections;

public class BackgroundMusicPlayer : MonoBehaviour
{
    [Header("Music Settings")]
    public AudioClip backgroundMusic;
    public float fadeInDuration = 1.5f;
    public float volume = 0.7f;
    public bool loopMusic = true;
    public float startDelay = 0.5f;
    
    [Header("Status")]
    public bool isPlaying = false;
    
    // Audio source for background music
    private AudioSource musicSource;
    
    void Awake()
    {
        // Create audio source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusic;
        musicSource.loop = loopMusic;
        musicSource.volume = 0;  // Start silent
        musicSource.playOnAwake = false;
    }
    
    // Call this to start the music with fade-in
    public void StartMusic()
    {
        if (!isPlaying && backgroundMusic != null)
        {
            // Start the music
            StartCoroutine(FadeInMusic());
            isPlaying = true;
        }
    }
    
    // Fade in the music smoothly
    private IEnumerator FadeInMusic()
    {
        // Optional delay before starting
        if (startDelay > 0)
            yield return new WaitForSeconds(startDelay);
            
        // Start the music
        musicSource.Play();
        
        // Fade in gradually
        float timeElapsed = 0;
        while (timeElapsed < fadeInDuration)
        {
            musicSource.volume = Mathf.Lerp(0, volume, timeElapsed / fadeInDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final volume is set
        musicSource.volume = volume;
    }
    
    // Optional: stop music with fade out
    public void StopMusic()
    {
        if (isPlaying)
        {
            StartCoroutine(FadeOutMusic());
            isPlaying = false;
        }
    }
    
    private IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;
        float timeElapsed = 0;
        
        while (timeElapsed < fadeInDuration)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, timeElapsed / fadeInDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        musicSource.Stop();
        musicSource.volume = 0;
    }
}