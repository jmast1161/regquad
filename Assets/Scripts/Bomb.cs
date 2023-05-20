using System;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public event Action<Bomb> ExplosionComplete;
    public Node Node;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ExplosionAnimation explosionAnimation;    
    [SerializeField] private AudioSource audioSource;
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
            audioSource.Play();
            explosionAnimator.Play("ExplosionTest", 0);
        }
    }

    private void OnExplosionComplete(AnimationEvent animationEvent)
    {
        ExplosionComplete?.Invoke(this);
    }
}
