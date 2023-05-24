using System;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public event Action<Bomb> ExplosionComplete;
    public Node Node;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ExplosionAnimation explosionAnimation;
    
    public void Init(Node node)
    {
        node.HasBomb = true;
        Node = node;
        explosionAnimation.ExplosionComplete += OnExplosionComplete;
    }

    public void PlayExplosion()
    {
        var explosionAnimator = GetComponentInChildren<Animator>();
        if(explosionAnimator != null)
        {
            explosionAnimator.Play("Explosion", 0);
        }
    }

    private void OnExplosionComplete(AnimationEvent animationEvent)
    {
        ExplosionComplete?.Invoke(this);
    }
}
