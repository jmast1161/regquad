using UnityEngine;

public class StopBlock : MonoBehaviour
{
    public Node Node;
    [SerializeField] private SpriteRenderer _renderer;

    public void Init(Node node)
    {
        Node = node;
        node.IsStopBlockNode = true;
    }
}
