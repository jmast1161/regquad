using UnityEngine;

public class ExplodeBombBlock : MonoBehaviour
{
    public Node Node;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private SpriteRenderer _bombRenderer;
    [SerializeField] private SpriteRenderer _crosshairRenderer;

    public void Init(Node node)
    {
        Node = node;
        node.IsExplodeBombNode = true;
    }
}
