using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text levelNumber;
    [SerializeField] private UnityEngine.UI.Button levelButton;
    [SerializeField] private bool active;
    [SerializeField] private int levelIndex;
    private int difficultyLevel = 1;

    public void SetDifficultyLevel(int difficultyLevel)
        => this.difficultyLevel = difficultyLevel;

    public void SubscribeCurrentLevelIndex(SoundManager soundManager, CurrentLevelIndex currentLevel)
    {
        levelButton.onClick.AddListener(() =>
        {
            soundManager.PlayConfirmSound();
            currentLevel.CurrentLevel = levelIndex;
            currentLevel.DifficultyLevel = difficultyLevel;
            SceneManager.LoadSceneAsync("GameScene");
        });
    }

    private void Start()
    {
        levelButton.interactable = active;
    }
}
