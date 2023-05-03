using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public bool GoalActive = false;
    public Node Node;
    [SerializeField] private SpriteRenderer _renderer;
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
            _renderer.color = Color.cyan;
        }
        else
        {            
            _renderer.color = Color.yellow;
        }
    }
}
