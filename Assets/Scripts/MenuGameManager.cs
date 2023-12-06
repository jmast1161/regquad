using UnityEngine;

public class MenuGameManager : MonoBehaviour
{
    private CurrentLevelIndex currentLevel;
    [SerializeField] private Animator mainMenuPanelAnimator;
    [SerializeField] private Animator levelSelectPanelAnimator;
    [SerializeField] private Animator settingsPanelAnimator;
    [SerializeField] private Animator creditsPanelAnimator;
    [SerializeField] private UnityEngine.UI.Button mainMenuPlayButton;
    [SerializeField] private UnityEngine.UI.Button creditsButton;
    [SerializeField] private UnityEngine.UI.Button creditsBackButton;
    [SerializeField] private UnityEngine.UI.Button mainMenuQuitButton;
    [SerializeField] private UnityEngine.UI.Button[] levelSelectBackButtons;
    [SerializeField] private UnityEngine.UI.Button[] levelSelectNextButtons;
    [SerializeField] private LevelSelectPanel[] levelSelectPanels;
    [SerializeField] private UnityEngine.UI.Button settingsButton;
    [SerializeField] private UnityEngine.UI.Button settingsBackButton;
    private SoundManager soundManager;
    [SerializeField] private CurrentLevelIndex currentLevelPrefab;
    [SerializeField] private SoundManager soundManagerPrefab;
    private bool mainMenuHidden;

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

        mainMenuPanelAnimator.SetTrigger("Show");
        mainMenuQuitButton.onClick.AddListener(() =>
        {
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

            if (!mainMenuHidden)
            {
                mainMenuPanelAnimator.SetTrigger("Hide");
                levelSelectPanelAnimator.SetTrigger("ShowRight");
                mainMenuHidden = true;
            }
        });

        settingsButton.onClick.AddListener(() =>
        {
            soundManager.PlayConfirmSound();
            if (!mainMenuHidden)
            {
                mainMenuPanelAnimator.SetTrigger("Hide");
                settingsPanelAnimator.SetTrigger("Show");
                mainMenuHidden = true;
            }
        });

        settingsBackButton.onClick.AddListener(() =>
        {
            mainMenuHidden = false;
            soundManager.PlayDeclineSound();
            soundManager.SaveAudioPreferences();
        });

        creditsButton.onClick.AddListener(() =>
        {
            soundManager.PlayConfirmSound();
            if (!mainMenuHidden)
            {
                mainMenuPanelAnimator.SetTrigger("Hide");
                creditsPanelAnimator.SetTrigger("Show");
                mainMenuHidden = true;
            }
        });

        creditsBackButton.onClick.AddListener(() =>
        {
            mainMenuHidden = false;
            soundManager.PlayDeclineSound();
        });

        for (int i = 0; i < levelSelectBackButtons.Length; ++i)
        {
            if (i == 0)
            {
                levelSelectBackButtons[i].onClick.AddListener(() =>
                {
                    soundManager.PlayDeclineSound();
                    mainMenuHidden = false;
                });
            }
            else
            {
                levelSelectBackButtons[i].onClick.AddListener(() =>
                {
                    soundManager.PlayDeclineSound();
                });
            }
        }

        for (int i = 0; i < levelSelectNextButtons.Length; ++i)
        {
            levelSelectNextButtons[i].onClick.AddListener(() =>
            {
                soundManager.PlayConfirmSound();
            });
        }
    }

    private void OnPanelMovementComplete(AnimationEvent animationEvent) 
    { 
        mainMenuHidden = false;
    }
}
