using System;
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
    public DropPreviewMarker previewMarker;

    public bool IsSetup => battleState == BattleState.Setup;

    [Header("Difficulty Scaling")]
    public int baseUnitCount = 4;
    public int unitIncreasePerRound = 1;

    public int hpIncreasePerRound = 2;
    public int damageIncreaseRound = 1;

    [Header("Economy/shop/Bench")]
    public int gold = 10;
    public ShopManager shop;
    public BenchManager bench;
    public ShopUI_TMP shopUI;

    private Unit selectedUnit;
    private int nextSpawnSeq = 1;
    private Dictionary<Unit, int> spawnSeq = new Dictionary<Unit, int>();

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
    //batch tf cache
    private readonly Dictionary<Unit, Vector3> cachedSetupPos = new Dictionary<Unit, Vector3>();
    private readonly Dictionary<Unit, Quaternion> cachedSetupRot = new Dictionary<Unit, Quaternion>();

    private int lastRewardeRound = 0;
    private bool lastRoundPlayerWin = false;

    [Header("Drag Drop Raycast")]
    public LayerMask tileLayerMask;
    public LayerMask benchSlotLayerMask;
    public LayerMask sellZoneLayerMask;

    [Header("Wave Table")]
    public WaveTableSo waveTable;

    private void Start()
    {
        GenerateGrid();
        if (shop != null) shop.Roll();
        if (shopUI == null) shopUI = FindAnyObjectByType<ShopUI_TMP>();

        shopUI?.Refresh();
        Debug.Log($"Gold : {gold}");
        previewMarker?.Hide();
    }
    private void Update()
    {
        if (battleState == BattleState.Setup)
        {
            HandleClick_SelectTileOrUnit();
            if (Input.GetMouseButton(1))
            {
                Unit u = GetUnitUnderMouse();
                if (u != null)
                {
                    TrySellUnit(u);
                }
                return;
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (shop != null)
                {
                    if (gold >= 2)
                    {
                        gold -= 2;
                        UI_Roll();
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha1)) UI_Buy(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) UI_Buy(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) UI_Buy(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) UI_Buy(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) UI_Buy(4);

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
        RegisterSpawnSeq(unit);
        unit.SetInBattle(true);

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
        CachePlacementBeforeBattle();

        SpawnEnemyWaveForRound(roundIndex);

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

            if (!unit.InBattle) continue;

            bool needRetarget = unit.currentTarget == null || unit.currentTarget.IsDead();

            if (needRetarget && Time.time >= unit.nextRetargetTime)
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

        RestorePlacementForSetup();   

        if (autoRollOnNewRound && shop != null)
        {
            UI_Roll();
        }
        Debug.Log($"Prepare Round {roundIndex}| Gold={gold} | AutoRoll={(autoRollOnNewRound ? "ON" : "OFF")}");
    }

    private IEnumerator CoprepareNextRound(float delay)
    {
        yield return new WaitForSeconds(delay);
        PrepareNextRound();
    }

    private void TryMergrAfterSpawn(Unit spawned)
    {
        if (spawned.star >= 3) return;
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

        Unit keep = PickKeepByPriority(candidates);

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
        TryMergrAfterSpawn(keep);
    }
    
    public void ClearTileReference(Unit unit)
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

    private Unit GetUnitUnderMouse()
    {
        if (Camera.main == null) return null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.collider.GetComponentInParent<Unit>();
        }
        return null;
    }

    public void UI_Buy(int offerIndex)
    {
        TryBuyFromShop(offerIndex);
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

        GameObject prefabToSpawn = (data.prefab != null) ? data.prefab : unitPrefab;
        GameObject go = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);
        Unit unit = go.GetComponent<Unit>();

        unit.ApplyData(data);
        unit.Init(TeamType.Player, roundIndex);
        unit.SetInBattle(false);
        RegisterSpawnSeq(unit);

        if (!bench.TryPlaceToEmptySlot(go))
        {
            Destroy(go);
            Debug.Log("Bench is full");
            return;
        }
        TryMergrAfterSpawn(unit);
        gold -= data.cost;
        shopUI?.Refresh();
        Debug.Log($"BUY {data.unitId} (cost {data.cost}) => Gold: {gold}");
    }

    public bool TrySellUnit(Unit u)
    {
        if (u == null) return false;
        if (!IsSetup) return false;

        int refund = u.GetSellRefund();
        gold += refund;

        ClearTileReference(u);

        if(bench != null)
        {
            var slot = bench.GetSlotByunit(u.gameObject);
            if (slot != null) slot.placedUnit = null;
        }
        Destroy(u.gameObject);
        shopUI?.Refresh();

        Debug.Log($"[SELL] {u.unitId} cost={u.cost} star={u.star} => +{refund} | Gold={gold}");
        return true;
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

    public void UI_Roll()
    {
        if (shop == null) return;

        shop.Roll();

        shopUI?.Refresh();
    }

    public bool IsPlayerZone(Tile t)
    {
        if (t == null) return false;
        int mid = height / 2;
        return t.gridPos.y < mid;
    }

    public bool IsEnemyZone(Tile t)
    {
        if (t == null) return false;
        int mid = height / 2;
        return t.gridPos.y >= mid;
    }

    private List<Tile> GetEmptyEnemyTiles()
    {
        var list = new List<Tile>();
        foreach (var t in tiles)
        {
            if (t == null) continue;
            if (!t.isPlaceable) continue;
            if (t.placedUnit != null) continue;
            if (!IsEnemyZone(t)) continue;
            list.Add(t);
        }
        return list;
    }

    private void SpawnEnemyWaveForRound(int round)
    {
        if (waveTable == null)
        {
            Debug.LogWarning("[Wave] waveTable is null");
            return;
        }

        var wave = waveTable.GetWave(round);
        if (wave == null || wave.spawns == null || wave.spawns.Count == 0)
        {
            Debug.LogWarning($"[Wave] no wave data for round {round}");
            return;
        }

        var emptyTiles = GetEmptyEnemyTiles();
        int cursor = 0;

        foreach (var entry in wave.spawns)
        {
            if (entry == null || entry.unit == null) continue;

            for (int i = 0; i < entry.count; i++)
            {
                if (cursor >= emptyTiles.Count)
                {
                    Debug.LogWarning("[Wave] Enemy zone is full. Cannot spwan more.");
                    return;
                }
                Tile tile = emptyTiles[cursor++];
                SpawnUnitOnTile(tile, entry.unit, TeamType.Enemy);
            }
        }
    }
    
    private void SpawnUnitOnTile(Tile tile, UnitData data, TeamType team)
    {
        if (tile == null || tile.placedUnit != null) return;

        GameObject prefabToSpawn = (data != null && data.prefab != null) ? data.prefab : unitPrefab;

        Vector3 pos = tile.transform.position + Vector3.up * 0.5f;
        GameObject go = Instantiate(prefabToSpawn, pos, Quaternion.identity);

        Unit u = go.GetComponent<Unit>();
        if (u == null) return;

        if (data != null) u.ApplyData(data);
        u.Init(team, roundIndex);
        RegisterSpawnSeq(u);
        u.SetInBattle(true);

        tile.placedUnit = go;
        TryMergrAfterSpawn(u);
    }

    public Tile GetTileUnderWorld(Vector3 worldPos)
    {
        Ray ray = new Ray(worldPos + Vector3.up * 5f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 20f, tileLayerMask))
        {
            return hit.collider.GetComponent<Tile>();
        }
        return null;
    }

    public bool IsOverSellZone(Vector3 worldPos)
    {
        Ray ray = new Ray(worldPos + Vector3.up * 5f, Vector3.down);
        return Physics.Raycast(ray, out _, 20f, sellZoneLayerMask, QueryTriggerInteraction.Collide);
    }


    public BenchSlot GetBenchSlotUnderWorld(Vector3 worldPos)
    {
        Ray ray = new Ray(worldPos + Vector3.up * 5f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 20f, benchSlotLayerMask))
        {
            return hit.collider.GetComponentInParent<BenchSlot>();
        }
        return null;
    }
   
    private void RegisterSpawnSeq(Unit u)
    {
        if (u == null) return;
        if (spawnSeq.ContainsKey(u)) return;
        spawnSeq[u] = nextSpawnSeq++;
    }
    private Unit PickKeepByPriority(List<Unit> candidates)
    {
        Unit bestBoard = null;
        int bestSeq = int.MaxValue;

        // 1) 보드에 있는 유닛이 하나라도 있으면 -> 보드 우선
        foreach (var u in candidates)
        {
            if (u == null) continue;
            if (IsOnBoard(u))
            {
                int seq = spawnSeq.TryGetValue(u, out var s) ? s : int.MaxValue;
                if (seq < bestSeq)
                {
                    bestSeq = seq;
                    bestBoard = u;
                }
            }
        }
        if (bestBoard != null)
            return bestBoard;

        if (bench != null)
        {
            Unit bestBench = null;
            int bestIndex = int.MaxValue;

            foreach (var u in candidates)
            {
                if (u == null) continue;
                BenchSlot slot = bench.GetSlotByunit(u.gameObject);
                if (slot == null) continue;

                if (slot.index < bestIndex)
                {
                    bestIndex = slot.index;
                    bestBench = u;
                }
            }
            if (bestBench != null)
                return bestBench;
        }
        return candidates[0];
    }

    private bool IsOnBoard(Unit u)
    {
        if (u == null || tiles == null) return false;
        foreach (Tile t in tiles)
        {
            if (t == null) continue;
            if (t.placedUnit == u.gameObject) return true;
        }
        return false;
    }

    private void CachePlacementBeforeBattle()
    {
        cachedSetupPos.Clear();
        cachedSetupRot.Clear();

        foreach (Unit u in FindObjectsOfType<Unit>())
        {
            if (u == null) continue;
            if (u.IsDead()) continue;
            if (u.team != TeamType.Player) continue;

            cachedSetupPos[u] = u.transform.position;
            cachedSetupRot[u] = u.transform.rotation;
        }

        Debug.Log($"[Cache] saved {cachedSetupPos.Count} Player unit placements");
    }

    private void RestorePlacementForSetup()
    {
        foreach (var kv in cachedSetupPos)
        {
            Unit u = kv.Key;
            if (u == null) continue;
            if (u.IsDead()) continue;

            u.transform.position = kv.Value;

            if (cachedSetupRot.TryGetValue(u, out var rot))
                u.transform.rotation = rot;
        }

        cachedSetupPos.Clear();
        cachedSetupRot.Clear();
    }

    public bool CanPlaceUnitOnTile(Unit u, Tile tile)
    {
        if (u == null || tile == null) return false;
        if (!tile.isPlaceable) return false;
        if (tile.placedUnit != null) return false;

        if (u.team == TeamType.Player && !IsPlayerZone(tile)) return false;

        return true;
    }
}
