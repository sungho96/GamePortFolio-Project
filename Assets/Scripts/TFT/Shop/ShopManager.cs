using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Pool")]
    public List<UnitData> pool = new List<UnitData>();

    [Header("Runtime offer (size 5)")]
    public UnitData[] offers = new UnitData[3];

    public void Roll()
    {
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning("Shop pool is empty");
            return;
        }

        for (int i = 0; i < offers.Length; i++)
        {
            offers[i] = pool[Random.Range(0, pool.Count)];
        }
        DebugOffers();
    }
    public UnitData GetOffer(int index)
    {
        if (index < 0 || index >= offers.Length) return null;
        return offers[index];
    }

    private void DebugOffers()
    {
        string msg = "[SHOP] ";
        for (int i = 0; i < offers.Length; i++)
        {
            string id = offers[i] ? offers[i].unitId : "null";
            msg += $"{i + 1}:{id}";
            if (i < offers.Length - 1) msg += " | ";
        }
        Debug.Log(msg);
    }

}
