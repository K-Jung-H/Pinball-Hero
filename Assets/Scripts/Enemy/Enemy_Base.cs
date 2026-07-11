using System;
using UnityEngine;

public enum EnemyState
{
    Moving,
    Attacking,
    Dead
}

public class Enemy_Base : MonoBehaviour
{
    private static int endlineLayer = -1;

    [SerializeField] private int hpMax;
    [SerializeField] private int hpCurrent;
    [SerializeField] private EnemyHpBar hpBar;
    [SerializeField] private Collider2D bodyCollider;

    private int damage;
    private float speed;
    private EnemyState state;
    private Transform attackTarget;

    public int Damage => damage;
    public bool IsDead => state == EnemyState.Dead;
    public EnemyState State => state;
    public Transform AttackTarget => attackTarget;

    public event Action<Enemy_Base> EnemyDamaged;
    public event Action<Enemy_Base> EnemyDied;
    public event Action<Enemy_Base> EndlineReached;
    public event Action<Enemy_Base, int> EnemyAttackCompleted;
    public event Action<Enemy_Base, int, int> HpChanged;

    private void Awake()
    {
        InitializeLayer();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (state != EnemyState.Moving || other.gameObject.layer != endlineLayer)
        {
            return;
        }

        ReachEndline();
    }

    private void Update()
    {
        if (state != EnemyState.Moving)
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
        state = EnemyState.Moving;

        if (bodyCollider != null)
        {
            bodyCollider.enabled = true;
        }

        if (hpBar != null)
        {
            hpBar.Initialize();
        }
    }

    public void SetAttackTarget(Transform target)
    {
        attackTarget = target;
    }

    public void TakeDamage(int value)
    {
        if (state != EnemyState.Moving)
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
        if (state != EnemyState.Moving)
        {
            return false;
        }

        state = EnemyState.Attacking;

        if (bodyCollider != null)
        {
            bodyCollider.enabled = false;
        }

        if (hpBar != null)
        {
            hpBar.Hide();
        }

        EndlineReached?.Invoke(this);

        return true;
    }

    public void CompleteEndlineAttack()
    {
        if (state != EnemyState.Attacking)
        {
            return;
        }

        state = EnemyState.Dead;
        EnemyAttackCompleted?.Invoke(this, damage);
        Despawn();
    }

    public void Despawn()
    {
        gameObject.SetActive(false);
    }

    private void Die()
    {
        if (state == EnemyState.Dead)
        {
            return;
        }

        Debug.Log("Enemy Dead!");
        state = EnemyState.Dead;

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

    private static void InitializeLayer()
    {
        if (endlineLayer != -1)
        {
            return;
        }

        endlineLayer = LayerMask.NameToLayer("Endline");

        if (endlineLayer == -1)
        {
            Debug.LogError("Endline layer is not defined.");
        }
    }
}
