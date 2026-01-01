using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{

    public float moveSpeed = 1.5f;
    public float lifeTime = 0.6f;

    private TMP_Text text;

    public void Init(int damage)
    {
        text = GetComponentInChildren<TMP_Text>();
        Debug.Log("DamagePopup Init : " + damage);
        text.text = damage.ToString();
    }

    private void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        lifeTime -= Time.deltaTime;

        if(lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
