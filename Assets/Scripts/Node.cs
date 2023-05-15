using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public bool IsOccupied;
    public bool HasTarget;
    public bool HasBomb;
    public bool IsGoalNode;
    public Vector2 Position => transform.position;
}
