using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompletePanel : MonoBehaviour
{
    public event Action<object> RestartLevelButtonClicked;
    public event Action<object> NextLevelButtonClicked;
    public event Action<object> MainMenuButtonClicked;
    
    [SerializeField] private Button restartLevelButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button mainMenuButton;

    // Start is called before the first frame update
    void Start()
    {
        restartLevelButton.onClick.AddListener(() => { RestartLevelButtonClicked?.Invoke(null);});
        nextLevelButton.onClick.AddListener(() => { NextLevelButtonClicked?.Invoke(null);});
        mainMenuButton.onClick.AddListener(() => { MainMenuButtonClicked?.Invoke(null);});
    }

    public void SetNextLevelButtonText(int currentLevelIndex)
    {
        if(currentLevelIndex % 25 == 0)
        {
            nextLevelButton.GetComponentInChildren<TMPro.TMP_Text >().text = "Next Difficulty";
        }
        else
        {
            nextLevelButton.GetComponentInChildren<TMPro.TMP_Text >().text = "Next Level";
        }
    }
}
