using UnityEngine;
using TMPro;

public class ShopUI_TMP : MonoBehaviour
{
    [Header("Refs")]
    private GridManager grid;
    private ShopManager shop;

    [Header("TMP Text")]
    public TMP_Text goldText;
    public TMP_Text[] offerTexts = new TMP_Text[5]; // size 5

    private void Awake()
    {
        if (grid == null)
            grid = FindAnyObjectByType<GridManager>();
        if (shop == null)
            shop = FindAnyObjectByType<ShopManager>();
    }
    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (grid != null && goldText != null)
            goldText.text = $"Gold: {grid.gold}";

        if (shop == null || offerTexts == null) return;

        for (int i = 0; i < offerTexts.Length; i++)
        {
            if (offerTexts[i] == null) continue;

            UnitData d = shop.GetOffer(i);
            offerTexts[i].text = (d == null)
                ? $"{i + 1}: (empty)"
                : $"{i + 1}: {d.unitId}  (${d.cost})";
        }
    }
}
