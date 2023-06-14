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
    [SerializeField] private UnityEngine.UI.Button[] levelSelectButtons;

    void Start()
    {
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
    }
}
