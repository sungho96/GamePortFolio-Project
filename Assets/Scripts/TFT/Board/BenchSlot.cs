using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenchSlot : MonoBehaviour
{
    public int index;
    public GameObject placedUnit;

    public bool HasUnit => placedUnit != null;

    public void place(GameObject unit)
    {
        placedUnit = unit;
        unit.transform.position = transform.position + Vector3.up * 0.5f;
    }

    public GameObject Take()
    {
        GameObject u = placedUnit;
        placedUnit = null;
        return u;
    }
}
