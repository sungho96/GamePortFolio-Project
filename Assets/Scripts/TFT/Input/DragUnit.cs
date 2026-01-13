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
        UpatePreview();
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
        if (grid != null && grid.previewMarker != null)
            grid.previewMarker.Hide();

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

        if (tile != null && tile.isPlaceable && tile.placedUnit == null 
            &&(unit.team != TeamType.Player || grid.IsPlayerZone(tile)))
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
        if (grid == null) grid = FindAnyObjectByType<GridManager>();
        if (grid == null) return false;
        return grid.IsOverSellZone(transform.position);
    }
    private void UpatePreview()
    {
        if (grid == null) grid = FindAnyObjectByType<GridManager>();
        if (grid == null || grid.previewMarker == null) return;
        
        if (!grid.IsSetup)
        {
            grid.previewMarker.Hide();
            return;
        }
        Vector3 p =transform.position;

        if (grid.IsOverSellZone(p))
        {
            grid.previewMarker.Show(p, DropPreviewMarker.Mode.Sell, true);
            return;
        }

        Tile tile = grid.GetTileUnderWorld(p);
        if (tile != null)
        {
            if (unit == null) unit = GetComponent<Unit>();
            bool valid = grid.CanPlaceUnitOnTile(unit, tile);
            grid.previewMarker.Show(tile.transform.position, DropPreviewMarker.Mode.Board, valid);
            return;
        }
        BenchSlot slot = grid.GetBenchSlotUnderWorld(p);
        if (slot != null)
        {
            bool valid = !slot.HasUnit || slot.placedUnit == gameObject;
            grid.previewMarker.Show(slot.transform.position, DropPreviewMarker.Mode.Bench, valid);
            return;
        }
        grid.previewMarker.Hide();
    }
}
