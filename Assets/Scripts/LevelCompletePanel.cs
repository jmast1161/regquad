using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompletePanel : MonoBehaviour
{
    public event Action<AnimationEvent> LevelCompletePanelShown;
    public event Action<object> RestartLevelButtonClicked;
    public event Action<object> NextLevelButtonClicked;
    public event Action<object> MainMenuButtonClicked;

    [SerializeField] private Button restartLevelButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private TMPro.TMP_Text headerText;
    [SerializeField] private PanelAnimation levelCompletePanelShown;
    // Start is called before the first frame update

    void Start()
    {
        restartLevelButton.onClick.AddListener(() => { RestartLevelButtonClicked?.Invoke(null); });
        nextLevelButton.onClick.AddListener(() => { NextLevelButtonClicked?.Invoke(null); });
        mainMenuButton.onClick.AddListener(() => { MainMenuButtonClicked?.Invoke(null); });

        levelCompletePanelShown.PanelShown += OnLevelCompletePanelShown;
    }

    public void InitializeButtons(CurrentLevelIndex currentLevel)
    {
        if (currentLevel.DifficultyLevel == 4 && currentLevel.CurrentLevel % 25 == 0)
        {
            creditsButton.gameObject.SetActive(true);
            nextLevelButton.gameObject.SetActive(false);
            headerText.text = "Thanks for playing!";
        }
        else
        {
            creditsButton.gameObject.SetActive(false);
            nextLevelButton.gameObject.SetActive(true);
            headerText.text = "Level Complete!";
            if (currentLevel.CurrentLevel % 25 == 0)
            {
                nextLevelButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Next Difficulty";
            }
            else
            {
                nextLevelButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Next Level";
            }
        }
    }

    private void OnLevelCompletePanelShown(AnimationEvent animationEvent) =>
        LevelCompletePanelShown?.Invoke(animationEvent);
}
