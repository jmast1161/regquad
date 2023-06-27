using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource moveAudioSource;
    [SerializeField] private AudioSource targetAudioSource;
    [SerializeField] private AudioSource goalAudioSource;
    [SerializeField] private AudioSource explosionAudioSource;

    [SerializeField] private UnityEngine.UI.Button musicIncreaseButton;
    [SerializeField] private UnityEngine.UI.Button musicDecreaseButton;
    [SerializeField] private UnityEngine.UI.Button effectsIncreaseButton;
    [SerializeField] private UnityEngine.UI.Button effectsDecreaseButton;
    [SerializeField] private TMPro.TMP_Text musicLevelDisplay;
    [SerializeField] private TMPro.TMP_Text effectsLevelDisplay;
    private int musicLevel = 10;
    private int effectsLevel = 10;

    private void Awake() 
    {
        DontDestroyOnLoad(this.gameObject);
    }
    
    private void Start() 
    {
        musicIncreaseButton.onClick.AddListener(OnMusicIncreaseClicked);
        musicDecreaseButton.onClick.AddListener(OnMusicDecreaseClicked);
        effectsIncreaseButton.onClick.AddListener(OnEffectsIncreaseClicked);
        effectsDecreaseButton.onClick.AddListener(OnEffectsDecreaseClicked);

        UpdateMusicVolume(musicLevel);
        UpdateEffectsVolume(effectsLevel);
    }

    private void OnMusicIncreaseClicked()
    {
        if(musicLevel == 10)
        {
            return;
        }

        UpdateMusicVolume(++musicLevel);
    }

    private void OnMusicDecreaseClicked()
    {
        if(musicLevel == 0)
        {
            return;
        }

        UpdateMusicVolume(--musicLevel);
    }

    private void OnEffectsIncreaseClicked()
    {
        if(effectsLevel == 10)
        {
            return;
        }

        UpdateEffectsVolume(++effectsLevel);
    }

    private void OnEffectsDecreaseClicked()
    {
        if(effectsLevel == 0)
        {
            return;
        }

        UpdateEffectsVolume(--effectsLevel);
    }

    private void UpdateMusicVolume(int newMusicLevel)
    {
        musicLevelDisplay.text = $"{newMusicLevel}";
        musicAudioSource.volume = newMusicLevel * 0.1f;
    }

    private void UpdateEffectsVolume(int newEffectsLevel)
    {
        effectsLevelDisplay.text = $"{effectsLevel}";
        var effectsLevelVolume = newEffectsLevel * 0.1f;
        moveAudioSource.volume = effectsLevelVolume;
        targetAudioSource.volume = effectsLevelVolume;
        goalAudioSource.volume = effectsLevelVolume;
        explosionAudioSource.volume = effectsLevelVolume;
    }

    public void PlayMusicAudioSource() =>
        musicAudioSource.Play();

    public void PlayMoveSound() =>
        moveAudioSource.Play();

    public void PlayTargetPickupSound(int blockDistance) =>
        targetAudioSource.PlayDelayed(0.15f * blockDistance);
    
    public void PlayGoalSound() =>
        goalAudioSource.Play();

    public void PlayExplosionSound() =>
        explosionAudioSource.Play();
}
