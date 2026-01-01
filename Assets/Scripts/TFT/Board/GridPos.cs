using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct GridPos
{
    public int x;
    public int y;
    public GridPos(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}