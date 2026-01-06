using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragUnit : MonoBehaviour
{
    private Camera cam;
    private bool isDragging;
    private Vector3 dragOffset;
    private Unit unit;
    private Vector3 startPos;
    private BenchSlot startBenchSlot;
    private GridManager grid;

    private void Awake()
    {
        cam = Camera.main;
        unit = GetComponent<Unit>();
    }

    private void OnMouseDown()
    {
        startPos = transform.position;

        if (grid == null)
            grid = FindAnyObjectByType<GridManager>();

        if (grid != null && grid.bench != null)
            startBenchSlot = grid.bench.GetSlotByunit(gameObject);
        else
            startBenchSlot = null;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        isDragging = true;

        Plane plane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hit = ray.GetPoint(enter);
            dragOffset = transform.position - hit;
        }
        else
        {
            dragOffset = Vector3.zero;
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging || cam == null) return;

        Plane plane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hit = ray.GetPoint(enter);
            transform.position = hit + dragOffset;
        }
    }

private void OnMouseUp()
{
    isDragging = false;

    if (grid == null)
        grid = FindAnyObjectByType<GridManager>();

    Debug.Log($"[DragUnit] OnMouseUp - pos={transform.position} grid={(grid != null)}");

    if (grid == null)
    {
        Debug.Log("[DragUnit] FAIL: grid is null -> revert");
        transform.position = startPos;
        return;
    }

    Tile tile = grid.GetTileUnderWorld(transform.position);
    Debug.Log($"[DragUnit] ray -> tile={(tile != null)}");

    if (tile == null)
    {
        Debug.Log("[DragUnit] FAIL: tile is null (raycast didn't hit Tile) -> revert");
        transform.position = startPos;
        return;
    }

    Debug.Log($"[DragUnit] tile.isPlaceable={tile.isPlaceable}");

    if (!tile.isPlaceable)
    {
        Debug.Log("[DragUnit] FAIL: tile is not placeable -> revert");
        transform.position = startPos;
        return;
    }

    Debug.Log($"[DragUnit] tile.placedUnit={(tile.placedUnit != null ? tile.placedUnit.name : "null")}");

    if (tile.placedUnit != null)
    {
        Debug.Log("[DragUnit] FAIL: tile already occupied -> revert");
        transform.position = startPos;
        return;
    }

    Debug.Log($"[DragUnit] startBenchSlot={(startBenchSlot != null ? startBenchSlot.name : "null")}");

    // ---- SUCCESS PATH ----
    if (startBenchSlot != null)
        startBenchSlot.placedUnit = null;

    if (unit == null)
        unit = GetComponent<Unit>();

    grid.ClearTileReference(unit);

    tile.placedUnit = gameObject;
    transform.position = tile.transform.position + Vector3.up * 0.5f;

    unit.SetInBattle(true);

    Debug.Log($"[DragUnit] SUCCESS: dropped to tile -> {tile.name}");
}

}
