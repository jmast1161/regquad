using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public bool IsOccupied;
    public bool HasTarget;
    public bool HasBomb;
    public bool IsGoalNode;
    public bool IsStopBlockNode;
    public bool IsDirectionBlockNode;
    public bool IsExplodeBombNode;
    
    public Vector2 Position => transform.position;
    [SerializeField] private SpriteRenderer _renderer;

    public void SetNodeOccupied()
    {
        IsOccupied = true;
        _renderer.color = new Color(_renderer.color.r, _renderer.color.g, _renderer.color.b, 100f);
    }

    public void SetGoalNode()
    {
        IsGoalNode = true;
        _renderer.color = new Color(_renderer.color.r, _renderer.color.g, _renderer.color.b, 0f);
    }
}
