using UnityEngine;
using UnityEngine.EventSystems;

public class RollButtonAutoBind : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GridManager grid;

    private void Awake()
    {
        if (grid == null)
            grid = FindAnyObjectByType<GridManager>();

        Debug.Log($"[RollButton] bind grid={(grid != null ? "OK" : "NULL")}");
    }

    // ✅ 버튼이 클릭되면 무조건 여기로 들어옵니다(클릭만 먹으면)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (grid == null)
            grid = FindAnyObjectByType<GridManager>();
            
        grid?.UI_Roll();
    }
}
