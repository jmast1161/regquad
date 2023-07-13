using UnityEngine;
using System;

[Serializable]
public class Level
{
    public int Index;
    public Vector2 PlayerPosition;
    public Vector2[] BombLocations;
    public Vector2[] OccupiedBlockLocations;
    public Vector2[] TargetLocations;
    public Vector2 GoalPosition;
    public int Moves;
}
