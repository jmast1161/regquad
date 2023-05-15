using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public Node Node;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void Init(Node node)
    {
        node.HasBomb = true;
        Node = node;
    }
}
