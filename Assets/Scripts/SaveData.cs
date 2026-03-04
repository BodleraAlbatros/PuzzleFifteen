using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PuzzleSaveData
{
    public int size;
    public int moves;
    public int emptyX;
    public int emptyY;
    public List<int> tileX = new List<int>();
    public List<int> tileY = new List<int>();
}