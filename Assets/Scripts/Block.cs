using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Node Node;

    public int X {get;set;}
    public int Y {get;set;}
    
    [SerializeField] private SpriteRenderer _renderer;

    public void Init(Node node, Color color, bool isPlayer)
    {
        if(!isPlayer)
        {
            node.IsOccupied = true;
        }

        Node = node;
        _renderer.color = color;
    }
}
