using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropPreviewMarker : MonoBehaviour
{
    public enum Mode { Board, Bench, Sell}
    [SerializeField] private SpriteRenderer ring;
    [SerializeField] private float yOffset = 0.02f;

    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;
    [SerializeField] private Color sellColor = new Color(1f, 0.6f, 0.1f, 1f);

    public void Show(Vector3 pos, Mode mode, bool valid)
    {
        gameObject.SetActive(true);
        transform.position = pos + Vector3.up * yOffset;
        if(ring != null)
        {
            if (mode == Mode.Sell) ring.color = sellColor;
            else ring.color = valid ? validColor : invalidColor;
        }
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
