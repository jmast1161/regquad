using System;
using UnityEngine;

public class ExplosionAnimation : MonoBehaviour
{
    public event Action<AnimationEvent> ExplosionComplete;
    public void OnExplosionComplete(AnimationEvent animationEvent)
    {
        ExplosionComplete?.Invoke(animationEvent);
    }
}
