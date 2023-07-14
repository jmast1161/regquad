using UnityEngine;

public class LevelSelectPanel : MonoBehaviour
{
    public int DifficultyLevel;
    public int CompletedLevels{ get; set; }

    public void InitializeLevelButtons(SoundManager soundManager, CurrentLevelIndex currentLevel) 
    {
        var levelButtons = GetComponentsInChildren<LevelSelectButton>();

        foreach(var levelButton in levelButtons)
        {
            levelButton.SetActive(CompletedLevels);
            levelButton.SetDifficultyLevel(DifficultyLevel);
            levelButton.SubscribeCurrentLevelIndex(soundManager, currentLevel);
        }
    }
}
