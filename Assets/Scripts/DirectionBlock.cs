using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockDirection
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
}

public class DirectionBlock : MonoBehaviour
{
    public Node Node;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private SpriteRenderer _directionRenderer;

    public BlockDirection BlockDirection;
    public void Init(Node node, BlockDirection blockDirection)
    {
        Node = node;
        node.IsDirectionBlockNode = true;
        this.BlockDirection = blockDirection;

        switch (blockDirection)
        {
            case BlockDirection.Right:
                transform.rotation = Quaternion.Euler(0, 0, 270);
                break;
            case BlockDirection.Down:
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case BlockDirection.Left:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case BlockDirection.Up:
            default:
                break;
        }
    }
}
