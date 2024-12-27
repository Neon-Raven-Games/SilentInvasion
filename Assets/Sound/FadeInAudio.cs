using System.Collections;
using UnityEngine;

public class FadeInAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float fadeDuration = 2.0f;
    [SerializeField] private float targetVolume = 1.0f;

    private void Start()
    {
        if (audioSource != null)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            Debug.LogError("No AudioSource component found.");
        }
    }

    private IEnumerator FadeIn()
    {
        audioSource.volume = 0;
        if (!audioSource.isPlaying) audioSource.Play();

        float startTime = Time.time;
        
        while (audioSource.volume < targetVolume)
        {
            var elapsed = Time.time - startTime;
            audioSource.volume = Mathf.Clamp01(elapsed / fadeDuration);

            yield return null; 
        }

        audioSource.volume = targetVolume;
    }
}