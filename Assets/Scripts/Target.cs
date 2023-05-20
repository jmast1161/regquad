using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public Node Node;
    [SerializeField] private SpriteRenderer _renderer;

    public void Init(Node node)
    {
        node.HasTarget = true;
        Node = node;
    }
}
