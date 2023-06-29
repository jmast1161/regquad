using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource moveAudioSource;
    [SerializeField] private AudioSource targetAudioSource;
    [SerializeField] private AudioSource goalAudioSource;
    [SerializeField] private AudioSource explosionAudioSource;
    public int MusicLevel { get; private set; } = 10;
    public int EffectsLevel { get; private set; } = 10;

    public void SaveAudioPreferences()
    {
        PlayerPrefs.SetInt("MusicLevel", MusicLevel);
        PlayerPrefs.SetInt("EffectsLevel", EffectsLevel);
    }

    private void Awake() 
    {
        DontDestroyOnLoad(this.gameObject);
        
        if(PlayerPrefs.HasKey("MusicLevel"))
        {
            MusicLevel = PlayerPrefs.GetInt("MusicLevel");
        }

        if(PlayerPrefs.HasKey("EffectsLevel"))
        {
            EffectsLevel = PlayerPrefs.GetInt("EffectsLevel");
        }
    }
    
    private void Start() 
    {
        UpdateMusicVolume(MusicLevel);
        UpdateEffectsVolume(EffectsLevel);
    }

    public void MusicIncrease()
    {
        if(MusicLevel == 10)
        {
            return;
        }

        UpdateMusicVolume(++MusicLevel);
    }

    public void MusicDecrease()
    {
        if(MusicLevel == 0)
        {
            return;
        }

        UpdateMusicVolume(--MusicLevel);
    }

    public void EffectsIncrease()
    {
        if(EffectsLevel == 10)
        {
            return;
        }

        UpdateEffectsVolume(++EffectsLevel);
    }

    public void EffectsDecrease()
    {
        if(EffectsLevel == 0)
        {
            return;
        }

        UpdateEffectsVolume(--EffectsLevel);
    }

    private void UpdateMusicVolume(int newMusicLevel) =>
        musicAudioSource.volume = newMusicLevel * 0.1f;

    private void UpdateEffectsVolume(int newEffectsLevel)
    {
        var effectsLevelVolume = newEffectsLevel * 0.1f;
        moveAudioSource.volume = effectsLevelVolume;
        targetAudioSource.volume = effectsLevelVolume;
        goalAudioSource.volume = effectsLevelVolume;
        explosionAudioSource.volume = effectsLevelVolume;
    }

    public void PlayMusicAudioSource()
    {
        if (!musicAudioSource.isPlaying)
        {
            musicAudioSource.Play();
        }
    }

    public void PlayMoveSound() =>
        moveAudioSource.Play();

    public void PlayTargetPickupSound(int blockDistance) =>
        targetAudioSource.PlayDelayed(0.15f * blockDistance);
    
    public void PlayGoalSound() =>
        goalAudioSource.Play();

    public void PlayExplosionSound() =>
        explosionAudioSource.Play();
}
