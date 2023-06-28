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
    [SerializeField] private UnityEngine.UI.Button levelSelectBackButton;
    [SerializeField] private UnityEngine.UI.Button[] levelSelectButtons;
    [SerializeField] private UnityEngine.UI.Button settingsBackButton;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private CurrentLevelIndex currentLevelPrefab;

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

        for(int i = 0; i < levelSelectButtons.Length; ++i)
        {
            var index = i;
            levelSelectButtons[i].onClick.AddListener(() =>
            {
                currentLevel.CurrentLevel = index + 1;
                SceneManager.LoadSceneAsync("GameScene");
            });
        }

        settingsBackButton.onClick.AddListener(() => 
        {
            soundManager.SaveAudioPreferences();
        });
    }
}
