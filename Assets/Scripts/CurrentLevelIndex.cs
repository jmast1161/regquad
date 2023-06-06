using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentLevelIndex : MonoBehaviour
{
    public int CurrentLevel { get; set; }

    private void Awake() 
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
