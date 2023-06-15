using UnityEngine;

public class CurrentLevelIndex : MonoBehaviour
{
    public int CurrentLevel { get; set; } = 1;

    private void Awake() 
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
