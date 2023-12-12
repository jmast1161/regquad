using System;
using UnityEngine;

public class PanelAnimation : MonoBehaviour
{
    public event Action<AnimationEvent> PanelShown;
    public void OnPanelShown(AnimationEvent animationEvent) =>
        PanelShown?.Invoke(animationEvent);
}
