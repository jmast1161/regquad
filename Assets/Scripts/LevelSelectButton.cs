using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectButton : MonoBehaviour
{

    [SerializeField] private TMPro.TMP_Text levelNumber;
    [SerializeField] private UnityEngine.UI.Button levelButton;
    [SerializeField] private int levelIndex = 0;
    private int difficultyLevel;

    public void SetDifficultyLevel(int difficultyLevel)
        => this.difficultyLevel = difficultyLevel;

    public void SubscribeCurrentLevelIndex(SoundManager soundManager, CurrentLevelIndex currentLevel)
    {
        levelButton.onClick.AddListener(() =>
        {
            soundManager.PlayConfirmSound();
            currentLevel.CurrentLevel = levelIndex + 1;
            currentLevel.DifficultyLevel = difficultyLevel;
            SceneManager.LoadSceneAsync("GameScene");
        });
    }

    public void SetActive(int completedLevels) =>
        levelButton.interactable = levelIndex <= completedLevels;
}
