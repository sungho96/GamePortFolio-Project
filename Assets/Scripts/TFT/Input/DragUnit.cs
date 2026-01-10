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

        if (grid != null && !grid.IsSetup) return;

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

        if (grid == null)
        {
            transform.position = startPos;
            return;
        }
        if (grid != null && grid.IsSetup)
        {
            if (unit == null) unit = GetComponent<Unit>();

            if (IsOverSellZone())
            {
                if (grid.TrySellUnit(unit))
                    return;
                transform.position = startPos;
                return;
            }
        }
        if (unit == null)
            unit = GetComponent<Unit>();

        Tile tile = grid.GetTileUnderWorld(transform.position);

        if (tile != null && tile.isPlaceable && tile.placedUnit == null)
        {
            if (startBenchSlot != null)
                startBenchSlot.placedUnit = null;

            grid.ClearTileReference(unit);

            tile.placedUnit = gameObject;
            transform.position = tile.transform.position + Vector3.up * 0.5f;

            unit.SetInBattle(true);
            return;
        }
        BenchSlot slot = grid.GetBenchSlotUnderWorld(transform.position);
        if (slot != null && !slot.HasUnit)
        {
            grid.ClearTileReference(unit);

            if (startBenchSlot != null && startBenchSlot != slot)
                startBenchSlot.placedUnit = null;

            slot.place(gameObject);

            unit.SetInBattle(false);
            return;
        }
        transform.position = startPos;
    }

    private void OnMouseOver()
    {
        if (grid == null || !grid.IsSetup) return;

        if(Input.GetMouseButtonDown(1))
        {
            if(unit == null) unit = GetComponent<Unit>();
            grid.TrySellUnit(unit);
           
        }

    }
    private bool IsOverSellZone()
    {
        if (grid == null)
            grid = FindAnyObjectByType<GridManager>();

        if (grid == null) return false;

        Ray ray = new Ray(transform.position+Vector3.up *5f, Vector3.down );
        return Physics.Raycast(
            ray,
            out _,
            20f,
            grid.sellZoneLayerMask,
            QueryTriggerInteraction.Collide
        );
        
    }

}
