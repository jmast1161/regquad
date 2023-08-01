using System;
using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    public event Action<object> RestartLevelButtonClicked;
    public event Action<object> MainMenuButtonClicked;
    [SerializeField] private UnityEngine.UI.Button restartLevelButton;
    [SerializeField] private UnityEngine.UI.Button mainMenuButton;
    
    // Start is called before the first frame update
    void Start()
    {
        restartLevelButton.onClick.AddListener(() => { RestartLevelButtonClicked?.Invoke(null);});
        mainMenuButton.onClick.AddListener(() => { MainMenuButtonClicked?.Invoke(null);});
    }
}
