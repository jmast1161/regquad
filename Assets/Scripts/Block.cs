using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Node Node;

    public int X {get;set;}
    public int Y {get;set;}
    
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource targetAudioSource;    
    [SerializeField] private AudioSource goalAudioSource;
    public void Init(Node node, Color color, bool isPlayer)
    {
        if(!isPlayer)
        {
            node.IsOccupied = true;
        }

        Node = node;
        _renderer.color = color;
    }

    public void PlayMoveSound()
    {
        audioSource.Play();
    }

    public void PlayTargetPickupSound(int blockDistance)
    {
        targetAudioSource.PlayDelayed(0.15f * blockDistance);
    }
    
    public void PlayGoalSound()
    {
        goalAudioSource.Play();
    }
}
