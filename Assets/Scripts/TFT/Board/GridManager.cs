using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Size")]
    public int width = 7;
    public int height = 4;
    public float tileSize = 1f;

    [Header("References")]
    public Tile tilePrefab;
    public GameObject unitPrefab;
    private Tile[,] tiles;
    private Tile selectedTile;
    private TeamType currentTeam = TeamType.Player;
    private BattleState battleState = BattleState.Setup;
    private int roundIndex = 1;

    [Header("Difficulty Scaling")]
    public int baseUnitCount = 4;
    public int unitIncreasePerRound = 1;

    public int hpIncreasePerRound = 2;
    public int damageIncreaseRound = 1;

    [Header("Day14 - Economy/shop/Bench")]
    public int gold = 10;
    public ShopManager shop;
    public BenchManager bench;

    private Unit selectedUnit;

    [Header("Round Income")]
    public int baseIncome = 5;
    public int winBonus = 1;
    public int lossBonus = 0;
    public int interestPer10 = 1;
    public int maxInterest = 5;

    [Header("Round Flow")]
    public bool autoRollOnNewRound = true;
    public bool autoAdvanceRound = false;
    public float roundEndDelay = 1.0f;

    private int lastRewardeRound = 0;
    private bool lastRoundPlayerWin = false;

    private void Start()
    {
        GenerateGrid();
        if (shop != null) shop.Roll();
        Debug.Log($"Gold : {gold}");
    }
    private void Update()
    {
        if (battleState == BattleState.Setup)
        {
            HandleClick_SelectTileOrUnit();

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (shop != null)
                {
                    shop.Roll();
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha1)) TryBuyFromShop(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) TryBuyFromShop(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) TryBuyFromShop(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) TryBuyFromShop(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) TryBuyFromShop(4);

            if (Input.GetKeyDown(KeyCode.M))
            {
                TryMoveSelectedUnitToSelectedTile();
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                currentTeam = TeamType.Player;
                TryPlaceUnit();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                currentTeam = TeamType.Enemy;
                TryPlaceUnit();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                TryRemoveUnit();
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                StartBattle();
            }
        }
        else if (battleState == BattleState.Battle)
        {
            UpdateBattle();
        }
        else if (battleState == BattleState.End)
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                PrepareNextRound();
            }
        }
    }
    private void GenerateGrid()
    {
        tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = new Vector3(
                    x * tileSize,
                    0f,
                    y * tileSize
                );

                Tile tile = Instantiate(
                    tilePrefab,
                    worldPos,
                    Quaternion.identity,
                    transform
                );

                tile.Init(new GridPos(x, y), true);
                tiles[x, y] = tile;
            }
        }
    }
    private void SelectTile(Tile tile)
    {
        if (selectedTile != null)
            selectedTile.SetSelected(false);

        selectedTile = tile;
        selectedTile.SetSelected(true);

        Debug.Log($"Selected Tile: ({tile.gridPos.x}, {tile.gridPos.y})");

    }
    private void SelectUnit(Unit u)
    {
        selectedUnit = u;
        Debug.Log($"Selected Unit: team={u.team}, id={u.unitId}, star={u.star}");
    }
    private void TryPlaceUnit()
    {
        if (selectedTile == null || !selectedTile.isPlaceable)
            return;

        if (selectedTile.placedUnit != null)
            return;

        Vector3 pos = selectedTile.transform.position + Vector3.up * 0.5f;
        GameObject go = Instantiate(unitPrefab, pos, Quaternion.identity);

        Unit unit = go.GetComponent<Unit>();
        unit.Init(currentTeam, roundIndex);

        selectedTile.placedUnit = go;
        TryMergrAfterSpawn(unit);
    }
    private void TryRemoveUnit()
    {
        if (selectedTile == null || selectedTile.placedUnit == null)
            return;

        Destroy(selectedTile.placedUnit);
        selectedTile.placedUnit = null;
    }
    private void StartBattle()
    {
        battleState = BattleState.Battle;
        Debug.Log("Battle Start");
    }
    private void UpdateBattle()
    {
        List<Unit> units = FindAllUnits();

        foreach (Unit unit in units)
        {
            if (unit == null || unit.IsDead())
                continue;
            bool needRetarget = unit.currentTarget == null || unit.currentTarget.IsDead();

            if(needRetarget && Time.time >= unit.nextRetargetTime)
            {
                unit.currentTarget = FindNearestEnemy(unit, units);
                unit.nextRetargetTime = Time.time + unit.retargetInterval;
            }

            Unit target = unit.currentTarget;
            if (target == null) continue;

            float chaseMaxDist = Mathf.Sqrt((width - 1) * (width - 1) + (height - 1) * (height - 1)) * tileSize + 0.5f;
            float dist = Vector3.Distance(unit.transform.position, target.transform.position);

            if (dist > chaseMaxDist)
            {
                unit.currentTarget = null;
                continue;
            }
            float attackRange = unit.attackRange;
            if (dist <= attackRange)
            {
                unit.TickAttack(Time.deltaTime, target);
            }
            else
            {
                Vector3 separation = unit.ComputeSeparation(units);
                unit.MoveTowards(target, Time.deltaTime, separation);
            }
        }
        CleanupDeadUnits(units);
        CheckBattleEnd(units);
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

            float dist = Vector3.Distance(unit.transform.position, other.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = other;
            }
        }
        return nearest;
    }
    private void CleanupDeadUnits(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            if (unit == null) continue;

            if (unit.IsDead())
            {
                if (unit.gameObject.GetComponent<UnitDeathEffect>() != null) continue;
                var death = unit.gameObject.AddComponent<UnitDeathEffect>();

                death.PlayAndDestroy();
                Debug.Log($"<color=red>[Death]</color> {unit.team} unit died");
            }
        }
    }

    private bool CheckBattleEnd(List<Unit> units)
    {
        bool playerAlive = false;
        bool enemyAlive = false;

        foreach (Unit unit in units)
        {
            if (unit == null || unit.IsDead()) continue;

            if (unit.team == TeamType.Player) playerAlive = true;
            else if (unit.team == TeamType.Enemy) enemyAlive = true;
        }
        if (!playerAlive || !enemyAlive)
        {
            if (battleState != BattleState.End)
            {
                battleState = BattleState.End;

                lastRoundPlayerWin = playerAlive;
                Debug.Log($"round {roundIndex} Result: {(lastRoundPlayerWin ? "Player win" : "Enemy Win")}");

                ApplyRoundIncome(lastRoundPlayerWin);
                CleanupAfterRound();

                if (autoAdvanceRound)
                    StartCoroutine(CoprepareNextRound(roundEndDelay));
            }
            return true;
        }
        return false;
    }
    private void PrepareNextRound()
    {
        roundIndex++;
        battleState = BattleState.Setup;

        if (autoRollOnNewRound && shop != null)
            shop.Roll();
        Debug.Log($"Prepare Round {roundIndex}| Gold={gold} | AutoRoll={(autoRollOnNewRound ? "ON" : "OFF")}");
    }
    private IEnumerator CoprepareNextRound(float delay)
    {
        yield return new WaitForSeconds(delay);
        PrepareNextRound();
    }
    /*
    private void AutoPlaceUnits()
    {
        ClearAllUnits();
        int spawnCount = baseUnitCount + (roundIndex - 1) * unitIncreasePerRound;
        spawnCount = Mathf.Min(spawnCount, height);

        for (int y =0; y < spawnCount; y++)
        {
            SpawnUnitAt(0,y, TeamType.Player); 
            SpawnUnitAt(width-1,y,TeamType.Enemy);
        }
        Debug.Log($"Round {roundIndex} Auto Placement Done (Units: {spawnCount})");
    }
    */
    private void SpawnUnitAt(int x, int y, TeamType team)
    {
        Tile tile = tiles[x, y];
        if (tile == null || tile.placedUnit != null)
            return;

        Vector3 pos = tile.transform.position + Vector3.up * 0.5f;
        GameObject go = Instantiate(unitPrefab, pos, Quaternion.identity);

        Unit unit = go.GetComponent<Unit>();
        unit.Init(team, roundIndex);
        unit.maxHp += (roundIndex - 1) * hpIncreasePerRound;
        unit.currentHp = unit.maxHp;

        unit.attackDamage += (roundIndex - 1) * damageIncreaseRound;

        tile.placedUnit = go;
        TryMergrAfterSpawn(unit);
    }
    /*
    private void ClearAllUnits()
    {
        foreach (Unit unit in FindObjectsOfType<Unit>())
        {
            Destroy(unit.gameObject);
        }

        foreach (Tile tile in tiles)
        {
            tile.placedUnit = null;
        }
    }
    */
    private void TryMergrAfterSpawn(Unit spawned)
    {
        if (spawned == null) return;

        List<Unit> candidates = new List<Unit>();
        foreach (Unit u in FindObjectsOfType<Unit>())
        {
            if (u == null) continue;
            if (u.team != spawned.team) continue;
            if (u.unitId != spawned.unitId) continue;
            if (u.star != spawned.star) continue;
            candidates.Add(u);
        }

        if (candidates.Count < 3) return;

        Unit keep = spawned;
        Unit remove1 = null;
        Unit remove2 = null;

        foreach (Unit u in candidates)
        {
            if (u == keep) continue;
            if (remove1 == null) remove1 = u;
            else if (remove2 == null) { remove2 = u; break; }
        }

        if (remove1 == null || remove2 == null) return;

        ClearTileReference(remove1);
        ClearTileReference(remove2);

        Destroy(remove1.gameObject);
        Destroy(remove2.gameObject);

        keep.IncreaseStar(roundIndex);
    }
    private void ClearTileReference(Unit unit)
    {
        if (unit == null) return;

        foreach (Tile t in tiles)
        {
            if (t == null) continue;
            if (t.placedUnit == unit.gameObject)
            {
                t.placedUnit = null;
                break;
            }
        }
    }
    private void HandleClick_SelectTileOrUnit()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        Unit u = hit.collider.GetComponentInParent<Unit>();
        if (u != null)
        {
            SelectUnit(u);
            return;
        }
        Tile tile = hit.collider.GetComponent<Tile>();
        if (tile != null)
        {
            SelectTile(tile);
        }
    }

    private void TryBuyFromShop(int offerIndex)
    {
        if (shop == null || bench == null || unitPrefab == null)
            return;

        UnitData data = shop.GetOffer(offerIndex);
        if (data == null)
        {
            Debug.LogWarning("offer is empty");
            return;
        }
        if (gold < data.cost)
        {
            Debug.Log($"Not enough gold. Gold={gold} cost={data.cost}");
            return;
        }

        GameObject go = Instantiate(unitPrefab, Vector3.zero, Quaternion.identity);
        Unit unit = go.GetComponent<Unit>();

        unit.unitId = data.unitId;
        unit.baseHp = data.baseHp;
        unit.baseAttack = data.baseAttack;

        unit.Init(TeamType.Player, roundIndex);

        if (!bench.TryPlaceToEmptySlot(go))
        {
            Destroy(go);
            Debug.Log("Bench is full");
            return;
        }
        gold -= data.cost;
        Debug.Log($"BUY {data.unitId} (cost {data.cost}) => Gold: {gold}");
    }

    private void TryMoveSelectedUnitToSelectedTile()
    {
        if(selectedUnit ==null)
        {
            Debug.Log("No unit selected.");
            return;
        }
        if(selectedTile == null)
        {
            Debug.Log("No tile selected");
            return;
        }
        var slot = bench != null ? bench.GetSlotByunit(selectedUnit.gameObject) : null;
        if (slot != null)
        {
            slot.placedUnit = null;
        }

        foreach (Tile t in tiles)
        {
            if (t != null && t.placedUnit == selectedUnit.gameObject)
            {
                t.placedUnit = null;
                break;
            }
        }
        selectedTile.placedUnit = selectedUnit.gameObject;
        selectedUnit.transform.position = selectedTile.transform.position + Vector3.up * 0.5f;

        Debug.Log("Unit moved to tile");
    }
    private void ApplyRoundIncome(bool playerWin)
    {
        if ((lastRewardeRound == roundIndex)) return;
        lastRewardeRound = roundIndex;

        int interest = Mathf.Min(maxInterest, (gold / 10) * interestPer10);
        int bonus = playerWin ? winBonus : lossBonus;

        int income = baseIncome + interest + bonus;
        gold += income;

        Debug.Log($"<color=green>[Income]</color> Round {roundIndex}| base={baseIncome}, interest={interest}, bonus={bonus} => +{income} | Gold={gold}");
    }
    private void CleanupAfterRound()
    {
        foreach (Unit u in FindObjectsOfType<Unit>())
        {
            if (u == null) continue;
            if (u.team == TeamType.Enemy)
            {
                ClearTileReference(u);
                Destroy(u.gameObject);
            }

            u.currentTarget = null;
            u.nextRetargetTime = 0f;
        }

        foreach (Tile t in tiles)
        {
            if (t == null) continue;
            if (t.placedUnit == null) continue;

            if (t.placedUnit == null)
                t.placedUnit = null;
        }

        Debug.Log("<color=cyan>[RoundEnd]</color> Cleanup done (enemy remvoed, targets removed");
    }
    public void UI_RollShop()
    {
        if (shop != null) shop.Roll();
    }

    public void UI_Buy0() => TryBuyFromShop(0);
    public void UI_Buy1() => TryBuyFromShop(1);
    public void UI_Buy2() => TryBuyFromShop(2);
    public void UI_Buy3() => TryBuyFromShop(3);
    public void UI_Buy4() => TryBuyFromShop(4);


}
