using System.Linq;
using System.IO;
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
    [SerializeField] private LevelSelectPanel[] levelSelectPanels;
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

        
        StreamReader reader = new StreamReader(Path.GetFullPath($@"{Application.dataPath}\Scripts\Levels.json"));
        string json = reader.ReadToEnd();
        var gameConfiguration = JsonUtility.FromJson<GameConfiguration>(json);

        foreach (var levelSelectPanel in levelSelectPanels)
        {
            levelSelectPanel.CompletedLevels = gameConfiguration.Difficulties
                .FirstOrDefault(x => x.DifficultyLevel == levelSelectPanel.DifficultyLevel)
                ?.CompletedLevels ?? 0;
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
