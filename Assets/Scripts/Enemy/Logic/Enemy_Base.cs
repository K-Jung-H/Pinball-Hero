using System;
using UnityEngine;

public class Enemy_Base : MonoBehaviour
{
    private enum EnemyState
    {
        Moving,
        Attacking,
        Dead
    }

    private static int endlineLayer = -1;

    [SerializeField] private SpriteHpBar hpBar;
    [SerializeField] private Collider2D bodyCollider;
    [SerializeField] private Collider2D hitTriggerCollider;
    [SerializeField] private StatusController statusController;

    private int damage;
    private int hpMax;
    private int hpCurrent;
    private float speed;
    private EnemyState state;
    private Transform attackTarget;
    private bool isDespawned;

    public bool CanReceiveDamage => state == EnemyState.Moving;
    public Transform AttackTarget => attackTarget;
    public StatusController StatusController => statusController;
    public Collider2D BodyCollider => bodyCollider;
    public Vector2 Center => hitTriggerCollider != null
        ? hitTriggerCollider.transform.TransformPoint(hitTriggerCollider.offset)
        : transform.position;

    public event Action<Enemy_Base> EnemyDamaged;
    public event Action<Enemy_Base> EnemyDied;
    public event Action<Enemy_Base> EndlineReached;
    public event Action<Enemy_Base, int> EnemyAttackCompleted;
    public event Action<Enemy_Base> Despawned;

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
        
        float moveSpeedMultiplier = statusController != null
            ? statusController.MoveSpeedMultiplier
            : 1f;

        transform.position += Vector3.down * (speed * moveSpeedMultiplier) * Time.deltaTime;
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
        isDespawned = false;

        SetCollidersEnabled(true);

        if (statusController != null)
        {
            statusController.Initialize(this);
        }

        if (hpBar != null)
        {
            hpBar.Initialize(hpCurrent, hpMax);
            hpBar.Hide();
        }
    }

    public void SetAttackTarget(Transform target)
    {
        attackTarget = target;
    }

    public EnemyHitSide GetHitSide(Vector2 hitPoint)
    {
        Collider2D referenceCollider = hitTriggerCollider != null
            ? hitTriggerCollider
            : bodyCollider;

        if (referenceCollider == null)
        {
            return EnemyHitSide.None;
        }

        Bounds bounds = referenceCollider.bounds;
        Vector2 offset = hitPoint - (Vector2)bounds.center;

        if (offset.sqrMagnitude <= 0.0001f)
        {
            return EnemyHitSide.None;
        }

        Vector2 extents = bounds.extents;
        float normalizedX = Mathf.Abs(offset.x) / Mathf.Max(0.0001f, extents.x);
        float normalizedY = Mathf.Abs(offset.y) / Mathf.Max(0.0001f, extents.y);

        if (normalizedY < normalizedX)
        {
            return EnemyHitSide.None;
        }

        return offset.y < 0f
            ? EnemyHitSide.Front
            : EnemyHitSide.Rear;
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
            hpBar.Show();
        }

        if (hpCurrent <= 0)
        {
            Die();
            return;
        }

        EnemyDamaged?.Invoke(this);
    }

    private void ReachEndline()
    {
        if (state != EnemyState.Moving)
        {
            return;
        }

        state = EnemyState.Attacking;

        SetCollidersEnabled(false);
        statusController?.ResetState();

        if (hpBar != null)
        {
            hpBar.Hide();
        }

        EndlineReached?.Invoke(this);
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
        if (isDespawned)
        {
            return;
        }

        isDespawned = true;
        state = EnemyState.Dead;
        attackTarget = null;
        SetCollidersEnabled(false);
        statusController?.ResetState();

        if (hpBar != null)
        {
            hpBar.Hide();
        }

        gameObject.SetActive(false);
        Despawned?.Invoke(this);
    }

    private void Die()
    {
        if (state == EnemyState.Dead)
        {
            return;
        }

        state = EnemyState.Dead;

        SetCollidersEnabled(false);
        statusController?.ResetState();

        if (hpBar != null)
        {
            hpBar.Hide();
        }

        EnemyDied?.Invoke(this);
    }

    private void SetCollidersEnabled(bool value)
    {
        if (bodyCollider != null)
        {
            bodyCollider.enabled = value;
        }

        if (hitTriggerCollider != null)
        {
            hitTriggerCollider.enabled = value;
        }
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
