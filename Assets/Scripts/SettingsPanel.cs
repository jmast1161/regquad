using UnityEngine;

public class SettingsPanel : MonoBehaviour
{
    private SoundManager soundManager;
    [SerializeField] private TMPro.TMP_Text musicLevelDisplay;
    [SerializeField] private TMPro.TMP_Text effectsLevelDisplay;
    [SerializeField] private TMPro.TMP_Text currentTrackDisplay;
    [SerializeField] private UnityEngine.UI.Button musicIncreaseButton;
    [SerializeField] private UnityEngine.UI.Button musicDecreaseButton;
    [SerializeField] private UnityEngine.UI.Button effectsIncreaseButton;
    [SerializeField] private UnityEngine.UI.Button effectsDecreaseButton;
    [SerializeField] private UnityEngine.UI.Button nextTrackButton;
    [SerializeField] private UnityEngine.UI.Button previousTrackButton;
    

    // Start is called before the first frame update
    void Start()
    {
        soundManager = GameObject.FindObjectOfType<SoundManager>();
        UpdateMusicDisplay();
        UpdateEffectsDisplay();
        UpdateCurrentTrackDisplay();
        musicIncreaseButton.onClick.AddListener(OnMusicIncreaseClicked);
        musicDecreaseButton.onClick.AddListener(OnMusicDecreaseClicked);
        effectsIncreaseButton.onClick.AddListener(OnEffectsIncreaseClicked);
        effectsDecreaseButton.onClick.AddListener(OnEffectsDecreaseClicked);
        nextTrackButton.onClick.AddListener(OnNextTrackButtonClicked);
        previousTrackButton.onClick.AddListener(OnPreviousTrackButtonClicked);
    }

    private void OnNextTrackButtonClicked()
    {
        soundManager.PlayConfirmSound();
        soundManager.NextMusicTrack();
        UpdateCurrentTrackDisplay();
    }

    private void OnPreviousTrackButtonClicked()
    {
        soundManager.PreviousMusicTrack();
        soundManager.PlayDeclineSound();
        UpdateCurrentTrackDisplay();
    }

    private void UpdateCurrentTrackDisplay() => 
        currentTrackDisplay.text = $"{soundManager.CurrentTrackIndex}";

    private void OnMusicIncreaseClicked()
    {
        soundManager.MusicIncrease();
        UpdateMusicDisplay();
    }

    private void OnMusicDecreaseClicked()
    {
        soundManager.MusicDecrease();
        UpdateMusicDisplay();
    }

    private void UpdateMusicDisplay() =>
        musicLevelDisplay.text = $"{soundManager.MusicLevel}";

    private void OnEffectsIncreaseClicked()
    {
        soundManager.PlayConfirmSound();
        soundManager.EffectsIncrease();
        UpdateEffectsDisplay();
    }

    private void OnEffectsDecreaseClicked()
    {
        soundManager.PlayDeclineSound();
        soundManager.EffectsDecrease();
        UpdateEffectsDisplay();
    }

    private void UpdateEffectsDisplay() =>
        effectsLevelDisplay.text = $"{soundManager.EffectsLevel}";

}
