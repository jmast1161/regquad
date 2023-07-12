using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuGameManager : MonoBehaviour
{
    private CurrentLevelIndex currentLevel;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelSelect;
    [SerializeField] private Animator mainMenuEntryAnimator;
    [SerializeField] private UnityEngine.UI.Button mainMenuPlayButton;
    [SerializeField] private UnityEngine.UI.Button mainMenuQuitButton;
    [SerializeField] private UnityEngine.UI.Button[] levelSelectBackButtons;
    [SerializeField] private UnityEngine.UI.Button[] levelSelectNextButtons;
    [SerializeField] private UnityEngine.UI.Button[] levelSelectButtons;
    [SerializeField] private UnityEngine.UI.Button settingsButton;
    [SerializeField] private UnityEngine.UI.Button settingsBackButton;
    private SoundManager soundManager;
    [SerializeField] private CurrentLevelIndex currentLevelPrefab;
    [SerializeField] private SoundManager soundManagerPrefab;

    void Awake()
    {
        soundManager = GameObject.FindObjectOfType<SoundManager>();
        if (soundManager == null)
        {
            soundManager = Instantiate(soundManagerPrefab);
        }
    }

    void Start()
    {
        currentLevel = GameObject.FindObjectOfType<CurrentLevelIndex>();
        if (currentLevel == null)
        {
            currentLevel = Instantiate(currentLevelPrefab);
        }

        mainMenuEntryAnimator.Play("ShowPanel", 0);
        mainMenuQuitButton.onClick.AddListener(() => 
        {
            Debug.Log("quit game");
            Application.Quit();
        });

        soundManager.PlayMusicAudioSource();

        mainMenuPlayButton.onClick.AddListener(() => 
        {
            soundManager.PlayConfirmSound();
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

        var levelButtons = GameObject.FindObjectsOfType<LevelSelectButton>();

        foreach(var levelButton in levelButtons)
        {
            levelButton.SubscribeCurrentLevelIndex(soundManager, currentLevel);
        }

        for(int i = 0; i < levelSelectBackButtons.Length; ++i)
        {
            levelSelectBackButtons[i].onClick.AddListener(() => 
            {
                soundManager.PlayDeclineSound();
            });
        }

        for(int i = 0; i < levelSelectNextButtons.Length; ++i)
        {
            levelSelectNextButtons[i].onClick.AddListener(() => 
            {
                soundManager.PlayConfirmSound();
            });
        }
    }
}
