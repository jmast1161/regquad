using UnityEngine;

public class Block : MonoBehaviour
{
    public Node Node;

    [SerializeField] private SpriteRenderer _renderer;
    
    public void Init(Node node)
    {
        Node = node;
    }
}
