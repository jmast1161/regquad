using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public bool IsOccupied;
    public bool HasTarget;
    public Vector2 Position => transform.position;
}
