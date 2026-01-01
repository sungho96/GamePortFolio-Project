using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenchManager : MonoBehaviour
{
    public List<BenchSlot> slots = new List<BenchSlot>();
    [Header("Auto Layout")]
    public bool autoLayoutOnStart = true;
    public float slotSpacing = 1.2f;
    public Vector3 localStart = new Vector3(-3f, 0f, -3f);

    private void Start()
    {
        if (!autoLayoutOnStart) return;

        for(int i =0; i< slots.Count; i++)
        {
            if (slots[i] == null) continue;
            slots[i].transform.localPosition = localStart + Vector3.right * slotSpacing * i;
        }
    }
    public bool TryPlaceToEmptySlot(GameObject unit)
    {
        foreach(var s in slots)
        {
            if (s == null) continue;
            if(!s.HasUnit)
            {
                s.place(unit);
                return true;
            }
        }
        return false;
    }
    public BenchSlot GetSlotByunit(GameObject unit)
    {
        foreach( var s in slots)
        {
            if (s != null && s.placedUnit ==unit)
                return s;
        }
        return null;
    }
}
