using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuGameManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelSelect;
    [SerializeField] private CurrentLevelIndex currentLevel;
    [SerializeField] private Animator mainMenuEntryAnimator;
    [SerializeField] private Animator levelSelectEntryAnimator;
    [SerializeField] private UnityEngine.UI.Button mainMenuPlayButton;
    [SerializeField] private UnityEngine.UI.Button mainMenuQuitButton;
    [SerializeField] private UnityEngine.UI.Button levelSelectBackButton;

    void Start()
    {
        //mainMenuEntryAnimator.Play("LevelCompletePanel", 0);
        mainMenuPlayButton.onClick.AddListener(() => 
        {
            //levelSelectEntryAnimator.Play("ShowLevelSelectPanel", 0);
        });

        mainMenuQuitButton.onClick.AddListener(() => 
        {
            Debug.Log("quit game");
            Application.Quit();
        });

        //currentLevel.CurrentLevel = 2;
        //SceneManager.LoadSceneAsync("GameScene");

        levelSelectBackButton.onClick.AddListener(() => 
        {
            //levelSelectEntryAnimator.Play("HideLevelSelectPanel", 0);
            Debug.Log("back clicked");
            //mainMenuEntryAnimator.Play("LevelCompletePanel", 0);
        });
    }
}
