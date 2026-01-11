using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public GridPos gridPos;
    public bool isPlaceable;

    public GameObject placedUnit;

    [SerializeField] private Renderer rend;
    [SerializeField] private Color placeableColor = Color.cyan;
    [SerializeField] private Color blockedColor = Color.red;
    [SerializeField] private Color SelectedColor = Color.blue;

    private bool isSelected = false;

    public void Init(GridPos pos, bool placeable)
    {
        gridPos = pos;
        isPlaceable = placeable;
        isSelected = false;
        UpdateColor();
    }
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateColor();
    }
    
    private void UpdateColor()
    {
        if (rend == null)
            rend = GetComponent<Renderer>();
        if (isSelected)
            rend.material.color = SelectedColor;
        else
            rend.material.color = isPlaceable ? placeableColor : blockedColor;
    }
}
