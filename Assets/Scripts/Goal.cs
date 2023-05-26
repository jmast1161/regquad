using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public bool GoalActive = false;
    public Node Node;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Sprite[] sprites;
    public void Init(Node node)
    {
        Node = node;
        SetGoalState(false);
    }

    public void SetGoalState(bool active)
    {
        GoalActive = active;
        if(GoalActive)
        {
            _renderer.sprite = sprites[0];
        }
        else
        {
            _renderer.sprite = sprites[1];
        }
    }
}
