using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public TeamType team;
    [Header("Stats")]
    public int maxHp = 10;
    public int currentHp;
    public int attackDamage = 2;
    public float attackRange = 1.5f;
    public float attackInterval = 1.0f;
    public float moveSpeed = 2.5f;
    public float separationRadius = 0.6f;
    public float separationForce = 2.0f;
    private float attackTimer;

    [Header ("Scaling")]
    public int baseHp = 10;
    public int baseAttack = 2;

    [Header("Merge")]
    public string unitId = "Default";
    public int star = 1;

    [SerializeField] private Renderer rend;
    [SerializeField] private Color playerColor = Color.cyan;
    [SerializeField] private Color enemyColr = Color.red;

    [Header("UI")]
    [SerializeField] private Transform hpBarFill;
    [SerializeField] private Transform attackCooldownFill;

    [Header("HP UI Lerp")]
    [SerializeField] private float hpLerpSpeed = 8f;
    private float disPlayedHpRatio = 1f;

    [Header("UI Effects")]
    [SerializeField] private GameObject damagePopupPrefab;

    public void Init(TeamType teamType, int round)
    {
        team = teamType;

        int hpBonus = round * 2;
        int atkBonus = round;

        maxHp = baseHp + hpBonus;
        attackDamage = baseAttack + atkBonus;

        currentHp = maxHp;
        attackTimer = 0f;

        UpdateColor();

        ApplyStarBonus(round);

        disPlayedHpRatio = 1f;

        UpdateHpBar();

        Debug.Log($"{team} Unit Init | Hp:{maxHp}, ATK:{attackDamage}");
    }
    private void Update()
    {
        UpdateHpBar();
    }

    private void UpdateColor()
    {
        if (rend == null)
            rend = GetComponentInChildren<Renderer>();
        rend.material.color = (team == TeamType.Player) ? playerColor : enemyColr;
    }

    public bool IsDead()
    {
        return currentHp <= 0;
    }
    public void TakeDamage(int damage)
    {
        currentHp = Mathf.Max(currentHp - damage, 0);

        SpawnDamagePopup(damage);
        StartCoroutine(HitFlash());
    }
    public void TickAttack(float deltaTime, Unit target)
    {
        if (target == null || IsDead() || target.IsDead()) return;
       
        attackTimer += deltaTime;
        UpdateCooldownBar();
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0;
            target.TakeDamage(attackDamage);
            UpdateCooldownBar();
        }
    }
    public void MoveTowards(Unit target, float deltaTime, Vector3 separtion)
    {
        if (target == null) return;

        Vector3 dir = (target.transform.position - transform.position).normalized;
        Vector3 velocity = (dir * moveSpeed) + separtion;

        transform.position += velocity * deltaTime;
    }
    public Vector3 ComputeSeparation(List<Unit> units)
    {
        Vector3 force = Vector3.zero;

        foreach (Unit other in units)
        {
            if (other == null || other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if(dist > 0f && dist <separationRadius)
            {
                Vector3 away = (transform.position - other.transform.position).normalized;
                force += away * (separationRadius - dist);
            }
        }
        return force * separationForce;
    }
    public void IncreaseStar(int round)
    {
        star = Mathf.Clamp(star + 1, 1, 3);

        float scale = 1f + 0.25f*(star - 1);
        transform.localScale = Vector3.one * scale;

        ApplyStarBonus(round);

        Debug.Log($"{team} Merge Success UnitID={unitId},star={star},HP={maxHp},atk{attackDamage}");
    }
    private void ApplyStarBonus(int round)
    {
        float mult = (star == 1) ? 1f : (star == 2 ? 1.5f : 2.2f);
        int hpBonus = round * 2;
        int atkBonus = round;

        int prevMaxHp = maxHp;

        maxHp = Mathf.RoundToInt((baseHp + hpBonus) * mult);
        attackDamage = Mathf.RoundToInt((baseAttack + atkBonus) * mult);

        currentHp = Mathf.Min(currentHp, maxHp);
    }
    private void UpdateHpBar()
    {
        if (hpBarFill == null) return;

        float targetratio = Mathf.Clamp01((float)currentHp / maxHp);
        disPlayedHpRatio = Mathf.Lerp(disPlayedHpRatio, targetratio, Time.deltaTime * hpLerpSpeed);
        Vector3 baseScale = hpBarFill.localScale;
        hpBarFill.localScale = new Vector3(disPlayedHpRatio, baseScale.y, baseScale.z);

        Vector3 p = hpBarFill.localPosition;
        hpBarFill.localPosition = new Vector3(
            -(1f - disPlayedHpRatio) * 0.5f,
            p.y,
            p.z
        );
    }
    private void UpdateCooldownBar()
    {
        if (attackCooldownFill == null) return;

        float ratio = Mathf.Clamp01(attackTimer / attackInterval);
        Vector3 baseScale = attackCooldownFill.localScale;
        attackCooldownFill.localScale = new Vector3(ratio, baseScale.y, baseScale.z);
    }
    private void SpawnDamagePopup(int damage)
    {
        if (damagePopupPrefab == null) return;

        Vector3 pos = transform.position + Vector3.up * 1.5f;
        GameObject go = Instantiate(damagePopupPrefab, pos, Quaternion.identity);

        DamagePopup popup = go.GetComponent<DamagePopup>();
        if (popup != null)
        {
            popup.Init(damage);
        }
    }
    private IEnumerator HitFlash()
    {
        if (rend == null) yield break;

        Color original = rend.material.color;
        rend.material.color = Color.white;

        yield return new WaitForSeconds(0.08f);

        rend.material.color = original;
    }
}
