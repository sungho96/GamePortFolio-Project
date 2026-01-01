using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Pool")]
    public List<UnitData> pool = new List<UnitData>();

    [Header("Runtime offer (size 3)")]
    public UnitData[] offers = new UnitData[3];

    public void Roll()
    {
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning("Shop pool is empty");
            return;
        }

        for (int i = 0; i < 3; i++)
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
        string a = offers[0] ? offers[0].unitId : null;
        string b = offers[1] ? offers[1].unitId : null;
        string c = offers[2] ? offers[2].unitId : null;
        Debug.Log($"[SHOP]1:{a} | 2:{b} | 3:{c}");
    }
}
