using System;
using UnityEngine;

public class Enemy_Base : MonoBehaviour
{
    [SerializeField] private int hpMax;

    [SerializeField] private int hpCurrent;
    private bool isDead;

    public event Action<Enemy_Base> EnemyDied;

    private void Awake()
    {
        hpCurrent = hpMax;
        isDead = false;
    }

    public void Initialize(int maxHp)
    {
        hpMax = maxHp;
        hpCurrent = hpMax;
        isDead = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        hpCurrent -= damage;

        if (hpCurrent <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }
        Debug.Log("Enemy Dead!");
        isDead = true;
        EnemyDied?.Invoke(this);
    }
}
