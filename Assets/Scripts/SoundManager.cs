using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicTrack1AudioSource;
    [SerializeField] private AudioSource musicTrack2AudioSource;
    [SerializeField] private AudioSource musicTrack3AudioSource;
    [SerializeField] private AudioSource moveAudioSource;
    [SerializeField] private AudioSource targetAudioSourcePrefab;
    [SerializeField] private AudioSource goalAudioSource;
    [SerializeField] private AudioSource explosionAudioSource;
    [SerializeField] private AudioSource confirmAudioSource;
    [SerializeField] private AudioSource declineAudioSource;
    private IList<AudioSource> targetAudioSources = new List<AudioSource>();
    private AudioSource currentMusicTrack;
    public int CurrentTrackIndex { get; private set; } = 1;
    public int MusicLevel { get; private set; } = 10;
    public int EffectsLevel { get; private set; } = 10;
    private const int maxTargetSounds = 5;
    public void SaveAudioPreferences()
    {
        PlayerPrefs.SetInt("MusicLevel", MusicLevel);
        PlayerPrefs.SetInt("EffectsLevel", EffectsLevel);
        PlayerPrefs.SetInt("CurrentTrack", CurrentTrackIndex);
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

        if(PlayerPrefs.HasKey("CurrentTrack"))
        {
            CurrentTrackIndex = PlayerPrefs.GetInt("CurrentTrack");
        }

        SetCurrentTrack();
    }

    public void NextMusicTrack()
    {
        if (CurrentTrackIndex == 3)
        {
            CurrentTrackIndex = 1;
        }
        else
        {
            ++CurrentTrackIndex;
        }

        SetCurrentTrack();
    }

    public void PreviousMusicTrack()
    {
        if (CurrentTrackIndex == 1)
        {
            CurrentTrackIndex = 3;
        }
        else
        {
            --CurrentTrackIndex;
        }

        SetCurrentTrack();
    }

    private void SetCurrentTrack()
    {
        if(currentMusicTrack != null)
        {
            currentMusicTrack.Stop();
        }

        switch(CurrentTrackIndex)
        {
            case 1:
                currentMusicTrack = musicTrack1AudioSource;
                break;
            case 2:
                currentMusicTrack = musicTrack2AudioSource;
                break;
            case 3:
                currentMusicTrack = musicTrack3AudioSource;
                break;
        }

        currentMusicTrack.volume = MusicLevel * 0.1f;
        PlayMusicAudioSource();
    }

    private void Start() 
    {
        for (int i = 0; i < maxTargetSounds; ++i)
        {
            targetAudioSources.Add(Instantiate(targetAudioSourcePrefab));
        }

        UpdateMusicVolume(MusicLevel);
        UpdateEffectsVolume(EffectsLevel);
    }

    public void MusicIncrease()
    {
        if(MusicLevel == 0)
        {
            currentMusicTrack.Play();
        }

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
            currentMusicTrack.Stop();
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
        currentMusicTrack.volume = newMusicLevel * 0.1f;

    private void UpdateEffectsVolume(int newEffectsLevel)
    {
        var effectsLevelVolume = newEffectsLevel * 0.1f;
        moveAudioSource.volume = effectsLevelVolume;
        goalAudioSource.volume = effectsLevelVolume;
        explosionAudioSource.volume = effectsLevelVolume;
        confirmAudioSource.volume = effectsLevelVolume;
        declineAudioSource.volume = effectsLevelVolume;

        foreach(var targetAudioSource in targetAudioSources)
        {
            targetAudioSource.volume = effectsLevelVolume;
        }
    }

    public void PlayMusicAudioSource()
    {
        if (!currentMusicTrack.isPlaying)
        {
            currentMusicTrack.Play();
        }
    }

    public void PlayMoveSound() =>
        moveAudioSource.Play();

    public void PlayTargetPickupSound(IList<int> blockDistances, int targetSoundQueueCount) 
    {

        for(int i = 0; i < targetSoundQueueCount; ++i)
        {
            var clipDelay = 0.15f * blockDistances[i];
            targetAudioSources[i].PlayScheduled(AudioSettings.dspTime + clipDelay);
        }
    }
    
    public void PlayGoalSound() =>
        goalAudioSource.Play();

    public void PlayExplosionSound() =>
        explosionAudioSource.Play();

    public void PlayConfirmSound() =>
        confirmAudioSource.Play();
    
    public void PlayDeclineSound() =>
        declineAudioSource.Play();
}
