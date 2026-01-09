using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitRarity
{
    common = 1,
    Uncommon = 2,
    Rare = 3,
    Epic = 4,
    Legendary = 5
}

[CreateAssetMenu(menuName = "TFT/UnitData")]
public class UnitData : ScriptableObject
{
    [Header("Identity")]
    public string unitId = "Default";
    public string displayName = "Default";

    [Header("shop")]
    public int cost = 1;
    public UnitRarity rarity = UnitRarity.common;

    [Header("Visual")]
    public GameObject prefab;
    public Sprite icon; //상점 카드이미지

    [Header("Base Stats")]
    public int baseHp = 10;
    public int baseAttack = 2;

    [Header("Combat Tuning (optional)")]
    public float attackRange = 1.5f;
    public float attackInterval = 1.0f;
    public float moveSpeed = 2.5f;
}
