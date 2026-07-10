using System;
using UnityEngine;

public class Enemy_Base : MonoBehaviour
{
    [SerializeField] private int hpMax;
    [SerializeField] private int hpCurrent;
    [SerializeField] private EnemyHpBar hpBar;
    [SerializeField] private Collider2D bodyCollider;

    private int damage;
    private float speed;
    private bool isDead;
    private bool hasReachedEndline;

    public int Damage => damage;
    public bool IsDead => isDead;

    public event Action<Enemy_Base> EnemyDamaged;
    public event Action<Enemy_Base> EnemyDied;
    public event Action<Enemy_Base> EndlineReached;
    public event Action<Enemy_Base, int, int> HpChanged;

    private void Awake()
    {
    }

    private void Update()
    {
        if (isDead || hasReachedEndline)
        {
            return;
        }
        
        transform.position += Vector3.down * speed * Time.deltaTime;
    }

    public void Initialize(EnemyDefinitionSO enemyDefinition)
    {
        if (enemyDefinition == null)
        {
            return;
        }

        damage = enemyDefinition.Damage;
        speed = enemyDefinition.Speed;
        hpMax = enemyDefinition.MaxHp;
        hpCurrent = hpMax;
        isDead = false;
        hasReachedEndline = false;

        if (bodyCollider != null)
        {
            bodyCollider.enabled = true;
        }

        if (hpBar != null)
        {
            hpBar.Initialize();
        }
    }

    public void TakeDamage(int value)
    {
        if (isDead || hasReachedEndline)
        {
            return;
        }

        int damageValue = Mathf.Max(0, value);

        if (damageValue <= 0)
        {
            return;
        }

        hpCurrent = Mathf.Max(0, hpCurrent - damageValue);

        if (hpBar != null)
        {
            hpBar.SetHp(hpCurrent, hpMax);
        }

        HpChanged?.Invoke(this, hpCurrent, hpMax);

        if (hpCurrent <= 0)
        {
            Die();
            return;
        }

        EnemyDamaged?.Invoke(this);
    }

    public bool ReachEndline()
    {
        if (isDead || hasReachedEndline)
        {
            return false;
        }

        hasReachedEndline = true;
        EndlineReached?.Invoke(this);

        return true;
    }

    public void CompleteEndlineAttack()
    {
        Die();
    }

    public void Despawn()
    {
        gameObject.SetActive(false);
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        Debug.Log("Enemy Dead!");
        isDead = true;

        if (bodyCollider != null)
        {
            bodyCollider.enabled = false;
        }

        if (hpBar != null)
        {
            hpBar.Hide();
        }

        EnemyDied?.Invoke(this);
    }
}
