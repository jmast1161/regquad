using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class MenuGameManager : MonoBehaviour
{
    private CurrentLevelIndex currentLevel;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelSelect;
    [SerializeField] private Animator mainMenuEntryAnimator;
    [SerializeField] private UnityEngine.UI.Button mainMenuPlayButton;
    [SerializeField] private UnityEngine.UI.Button creditsButton;
    [SerializeField] private UnityEngine.UI.Button creditsBackButton;
    [SerializeField] private UnityEngine.UI.Button mainMenuQuitButton;
    [SerializeField] private UnityEngine.UI.Button[] levelSelectBackButtons;
    [SerializeField] private UnityEngine.UI.Button[] levelSelectNextButtons;
    [SerializeField] private UnityEngine.UI.Button[] levelSelectButtons;
    [SerializeField] private LevelSelectPanel[] levelSelectPanels;
    [SerializeField] private UnityEngine.UI.Button settingsButton;
    [SerializeField] private UnityEngine.UI.Button settingsBackButton;
    private SoundManager soundManager;
    [SerializeField] private CurrentLevelIndex currentLevelPrefab;
    [SerializeField] private SoundManager soundManagerPrefab;
    [SerializeField] private TextAsset configuration;

    void Awake()
    {
        soundManager = GameObject.FindObjectOfType<SoundManager>();
        if (soundManager == null)
        {
            soundManager = Instantiate(soundManagerPrefab);
        }

        foreach (var levelSelectPanel in levelSelectPanels)
        {
            switch (levelSelectPanel.DifficultyLevel)
            {
                case 1:
                    levelSelectPanel.CompletedLevels = PlayerPrefs.GetInt("completed3x3Levels", 0);
                    break;
                case 2:
                    levelSelectPanel.CompletedLevels = PlayerPrefs.GetInt("completed4x4Levels", 0);
                    break;
                case 3:
                    levelSelectPanel.CompletedLevels = PlayerPrefs.GetInt("completed5x5Levels", 0);
                    break;
                case 4:
                    levelSelectPanel.CompletedLevels = PlayerPrefs.GetInt("completed6x6Levels", 0);
                    break;
            }
        }
    }

    void Start()
    {
        currentLevel = GameObject.FindObjectOfType<CurrentLevelIndex>();
        if (currentLevel == null)
        {
            currentLevel = Instantiate(currentLevelPrefab);
        }

        mainMenuEntryAnimator.SetTrigger("Show");
        mainMenuQuitButton.onClick.AddListener(() =>
        {
            Debug.Log("quit game");
            Application.Quit();
        });

        soundManager.PlayMusicAudioSource();

        mainMenuPlayButton.onClick.AddListener(() =>
        {
            soundManager.PlayConfirmSound();
            foreach (var levelSelectPanel in levelSelectPanels)
            {
                levelSelectPanel.InitializeLevelButtons(soundManager, currentLevel);
            }
        });

        settingsButton.onClick.AddListener(() =>
        {
            soundManager.PlayConfirmSound();
        });

        settingsBackButton.onClick.AddListener(() =>
        {
            soundManager.PlayDeclineSound();
            soundManager.SaveAudioPreferences();
        });

        creditsButton.onClick.AddListener(() =>
        {
            soundManager.PlayConfirmSound();
        });

        creditsBackButton.onClick.AddListener(() =>
        {
            soundManager.PlayDeclineSound();
        });

        for (int i = 0; i < levelSelectBackButtons.Length; ++i)
        {
            levelSelectBackButtons[i].onClick.AddListener(() =>
            {
                soundManager.PlayDeclineSound();
            });
        }

        for (int i = 0; i < levelSelectNextButtons.Length; ++i)
        {
            levelSelectNextButtons[i].onClick.AddListener(() =>
            {
                soundManager.PlayConfirmSound();
            });
        }
    }
}
