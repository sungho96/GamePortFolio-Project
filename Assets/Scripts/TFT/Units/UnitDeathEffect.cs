using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDeathEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.4f;

    private Renderer[] rends;
    private Vector3 startScale;

    private void Awake()
    {
        rends = GetComponentsInChildren<Renderer>();
        startScale = transform.localScale;
    }
    public void Play(System.Action onFinish)
    {
        StartCoroutine(Co_Death(onFinish));
    }

    private IEnumerator Co_Death(System.Action onFinish)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float ratio = 1f - (t / duration);
            transform.localScale = startScale * ratio;

            foreach (var r in rends)
            {
                if (r.material.HasProperty("_color"))
                {
                    Color c = r.material.color;
                    c.a = ratio;
                    r.material.color = c;
                }
            }

            yield return null;
        }
        onFinish?.Invoke();
    }
}
