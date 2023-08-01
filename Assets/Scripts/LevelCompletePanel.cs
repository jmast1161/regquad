using System;
using UnityEngine;

public class LevelCompletePanel : MonoBehaviour
{
    public event Action<object> RestartLevelButtonClicked;
    public event Action<object> NextLevelButtonClicked;
    public event Action<object> MainMenuButtonClicked;
    
    [SerializeField] private UnityEngine.UI.Button restartLevelButton;
    
    [SerializeField] private UnityEngine.UI.Button nextLevelButton;
    [SerializeField] private UnityEngine.UI.Button mainMenuButton;

    // Start is called before the first frame update
    void Start()
    {
        restartLevelButton.onClick.AddListener(() => { RestartLevelButtonClicked?.Invoke(null);});
        nextLevelButton.onClick.AddListener(() => { NextLevelButtonClicked?.Invoke(null);});
        mainMenuButton.onClick.AddListener(() => { MainMenuButtonClicked?.Invoke(null);});
    }
}
