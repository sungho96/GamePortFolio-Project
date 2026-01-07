using UnityEngine;
using UnityEngine.EventSystems;

public class RollButtonAutoBind : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GridManager grid;

    private void Awake()
    {
        if (grid == null)
            grid = FindAnyObjectByType<GridManager>();
    }

    // ✅ 버튼이 클릭되면 무조건 여기로 들어옵니다(클릭만 먹으면)
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[RollButton] clicked");
        if (grid == null)
            grid = FindAnyObjectByType<GridManager>();
        if (grid.gold >= 2)
        {
            grid.gold -= 2;
            grid?.UI_Roll();
        }
    }
}
