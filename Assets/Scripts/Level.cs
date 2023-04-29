using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Level
{
    public int Index;
    public int GridWidth;
    public int GridHeight;
    public Vector2[] OccupiedBlockLocations;
}
