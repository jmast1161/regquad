using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsPanel : MonoBehaviour
{
    private SoundManager soundManager;
    [SerializeField] private TMPro.TMP_Text musicLevelDisplay;
    [SerializeField] private TMPro.TMP_Text effectsLevelDisplay;
    [SerializeField] private UnityEngine.UI.Button musicIncreaseButton;
    [SerializeField] private UnityEngine.UI.Button musicDecreaseButton;
    [SerializeField] private UnityEngine.UI.Button effectsIncreaseButton;
    [SerializeField] private UnityEngine.UI.Button effectsDecreaseButton;
    

    // Start is called before the first frame update
    void Start()
    {
        soundManager = GameObject.FindObjectOfType<SoundManager>();
        UpdateMusicDisplay();
        UpdateEffectsDisplay();
        musicIncreaseButton.onClick.AddListener(OnMusicIncreaseClicked);
        musicDecreaseButton.onClick.AddListener(OnMusicDecreaseClicked);
        effectsIncreaseButton.onClick.AddListener(OnEffectsIncreaseClicked);
        effectsDecreaseButton.onClick.AddListener(OnEffectsDecreaseClicked);
    }

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
        soundManager.EffectsIncrease();
        UpdateEffectsDisplay();
    }

    private void OnEffectsDecreaseClicked()
    {
        soundManager.EffectsDecrease();
        UpdateEffectsDisplay();
    }

    private void UpdateEffectsDisplay() =>
        effectsLevelDisplay.text = $"{soundManager.EffectsLevel}";

}
