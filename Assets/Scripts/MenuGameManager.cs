using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuGameManager : MonoBehaviour
{
    private Canvas mainMenu;
    [SerializeField] private Canvas mainMenuPrefab;
    [SerializeField] private CurrentLevelIndex currentLevel;

    void Start()
    {
        mainMenu = Instantiate(mainMenuPrefab, new Vector3(0f, 0f), Quaternion.identity);
        var buttons = mainMenu.GetComponentsInChildren<UnityEngine.UI.Button>();
        var entryAnimator = mainMenu.GetComponentInChildren<Animator>();
        if(entryAnimator != null)
        {
            entryAnimator.Play("LevelCompletePanel", 0);
        }

        var playButton = buttons.FirstOrDefault(b => b.name == "Play");

        if(playButton != null)
        {
            playButton.onClick.AddListener(() => 
            {
                currentLevel.CurrentLevel = 2;
                SceneManager.LoadSceneAsync("GameScene");
            });
        }

        var quitButton = buttons.FirstOrDefault(b => b.name == "Quit");

        if(quitButton != null)
        {
            quitButton.onClick.AddListener(() => 
            {
                Application.Quit();
            });
        }
    }
}
