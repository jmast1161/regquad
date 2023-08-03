using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPickupAudioSource : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
