using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    [SerializeField] private List<AudioClip> gloveCatchClips;
    [SerializeField] private List<AudioClip> throwClips;
    [SerializeField] private List<AudioClip> strikeClips;
    [SerializeField] private float soundEffectsVolume = 1.0f;
    [SerializeField] private AudioClip trailClip;
    [SerializeField] private AudioClip fireClip;
    [SerializeField] private AudioClip bombDropClip;
    private static SFXManager _instance;

    public static void SetSfxVolume(float volume)
    {
        _instance.soundEffectsVolume = volume;
    }

    public static void PlayRandomStrike(AudioSource source, float volume = 1f)
    {
        source.volume = Mathf.Lerp(0, volume, _instance.soundEffectsVolume);
        source.PlayOneShot(_instance.strikeClips[
            Random.Range(0, _instance.strikeClips.Count)]);
    }

    public static void PlayFireBallTrail(AudioSource source, float volume)
    {
        source.volume = Mathf.Lerp(0, volume, _instance.soundEffectsVolume); 
        source.PlayOneShot(_instance.fireClip);
    }
    
    public static void PlayBombDropCatcher(AudioSource source, float volume)
    {
        source.volume = Mathf.Lerp(0, volume, _instance.soundEffectsVolume); 
        source.PlayOneShot(_instance.bombDropClip);
    }
    
    public static void PlayRegularBallTrail(AudioSource source, float volume = 1f)
    {
        source.volume = Mathf.Lerp(0, volume, _instance.soundEffectsVolume);
        
        var minPitch = 0.8f;
        var maxPitch = 1.6f;
        
        source.pitch = Mathf.Lerp(minPitch, maxPitch, Mathf.Clamp01(volume));
        source.PlayOneShot(_instance.trailClip);
    }

    public static void PlayRandomCatch(AudioSource source, float volume = 1f)
    {
        source.volume = Mathf.Lerp(0, volume, _instance.soundEffectsVolume);
        source.PlayOneShot(_instance.gloveCatchClips[
            Random.Range(0, _instance.gloveCatchClips.Count)]);
    }

    public static void PlayRandomThrow(AudioSource source, float volume = 1f)
    {
        if (_instance.throwClips.Count == 0) return;
        source.volume = Mathf.Lerp(0, volume, _instance.soundEffectsVolume);
        source.PlayOneShot(_instance.throwClips[
            Random.Range(0, _instance.throwClips.Count)]);
    }

    // Start is called before the first frame update
    private void Awake()
    {
        if (_instance != null && _instance != this) Destroy(gameObject);
        else if (_instance == null) _instance = this;
    }

    // Update is called once per frame
    void Update()
    {
    }

}