using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OfferCardView : MonoBehaviour
{
    [SerializeField] private int index;
    [SerializeField] private GridManager grid;

    private Button btn;
    private void Awake()
    {
        if (grid == null)
            grid = FindAnyObjectByType<GridManager>();
            
        btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (grid == null) return;
        grid.UI_Buy(index);
    }
}
