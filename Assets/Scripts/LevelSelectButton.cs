using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text levelNumber;
    [SerializeField] private UnityEngine.UI.Button levelButton;
    [SerializeField] private bool active;
    [SerializeField] private int levelIndex;

    public void SubscribeCurrentLevelIndex(SoundManager soundManager, CurrentLevelIndex currentLevel)
    {
        levelButton.onClick.AddListener(() =>
        {
            soundManager.PlayConfirmSound();
            currentLevel.CurrentLevel = levelIndex;
            SceneManager.LoadSceneAsync("GameScene");
        });
    }

    private void Start()
    {
        levelButton.interactable = active;
    }
}
