using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "TFT/UnitData")]
public class UnitData : ScriptableObject
{
    public string unitId = "Default";
    public int cost = 1;

    [Header("Base Stats")]
    public int baseHp = 10;
    public int baseAttack = 2;
}
