using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectPanel : MonoBehaviour
{
    public int DifficultyLevel;

    private void Start() 
    {
        var levelButtons = GetComponentsInChildren<LevelSelectButton>();

        foreach(var levelButton in levelButtons)
        {
            levelButton.SetDifficultyLevel(DifficultyLevel);
        }
    }
}
