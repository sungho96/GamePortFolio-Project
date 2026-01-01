using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    private void Update()
    {
        UpdateBattle();
    }
    private void UpdateBattle()
    {
        List<Unit> units = FindAllUnits();

        foreach (Unit unit in units)
        {
            if (unit == null || unit.IsDead())
                continue;

            Unit target = FindNearestEnemy(unit, units);
            if (target == null)
                continue;

            float dist = Vector3.Distance(
                unit.transform.position,
                target.transform.position
            );

            if (dist <= unit.attackRange)
            {
                unit.TickAttack(Time.deltaTime, target);
            }
        }
    }
    private List<Unit> FindAllUnits()
    {
        return new List<Unit>(FindObjectsOfType<Unit>());
    }
    private Unit FindNearestEnemy(Unit unit, List<Unit> units)
    {
        Unit nearest = null;
        float minDist = float.MaxValue;

        foreach (Unit other in units)
        {
            if (other == null || other.team == unit.team || other.IsDead())
                continue;
            float dist = Vector3.Distance(
                unit.transform.position,
                other.transform.position
            );
            if (dist < minDist)
            {
                minDist = dist;
                nearest = other;
            }
        }
        return nearest;
    }
}

