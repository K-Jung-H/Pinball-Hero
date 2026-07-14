using System;
using UnityEngine;

public class Ball_Base : MonoBehaviour
{
    private enum BallState
    {
        Ready,
        Flying,
        Returning
    }

    [Header("Components")]
    [SerializeField] private Rigidbody2D ballRigidbody;
    [SerializeField] private Collider2D ballCollider;

    private Transform returnTarget;

    private static int endlineLayer = -1;
    private static int shooterLayer = -1;
    private static int enemyTriggerLayer = -1;
    private static int enemySolidLayer = -1;
    private static int wallLayer = -1;

    private float currentSpeed;
    private BallState state;
    private bool hasExitedEndline;
    private int wallHitCount;
    private Collider2D ignoredCollider;
    private readonly BallRuntimeStat runtimeStat = new BallRuntimeStat();

    public event Action<Ball_Base> ReturnRequested;
    public event Action<Ball_Base, Enemy_Base, Vector2> EnemyHit;

    public BallType BallType => runtimeStat.BallType;
    public BallRuntimeStat RuntimeStat => runtimeStat;
    public virtual int CurrentHitDamage => runtimeStat.HitDamage;
    public Vector2 Velocity => ballRigidbody != null ? ballRigidbody.linearVelocity : Vector2.zero;

    protected Rigidbody2D BallRigidbody => ballRigidbody;

    protected void Awake()
    {
        CacheLayers();
    }

    private static void CacheLayers()
    {
        if (endlineLayer != -1
            && shooterLayer != -1
            && enemyTriggerLayer != -1
            && enemySolidLayer != -1
            && wallLayer != -1)
        {
            return;
        }

        if (endlineLayer == -1)
        {
            endlineLayer = LayerMask.NameToLayer("Endline");
        }

        if (shooterLayer == -1)
        {
            shooterLayer = LayerMask.NameToLayer("Shooter");
        }

        if (enemyTriggerLayer == -1)
        {
            enemyTriggerLayer = LayerMask.NameToLayer("Enemy Trigger");
        }

        if (enemySolidLayer == -1)
        {
            enemySolidLayer = LayerMask.NameToLayer("Enemy Solid");
        }

        if (wallLayer == -1)
        {
            wallLayer = LayerMask.NameToLayer("Wall");
        }

        if (endlineLayer == -1)
        {
            Debug.LogError("Endline layer is not defined.");
        }

        if (shooterLayer == -1)
        {
            Debug.LogError("Shooter layer is not defined.");
        }

        if (enemyTriggerLayer == -1)
        {
            Debug.LogError("Enemy Trigger layer is not defined.");
        }

        if (enemySolidLayer == -1)
        {
            Debug.LogError("Enemy Solid layer is not defined.");
        }

        if (wallLayer == -1)
        {
            Debug.LogError("Wall layer is not defined.");
        }
    }

    private void FixedUpdate()
    {
        UpdateIgnoredCollision();

        if (state == BallState.Ready || ballRigidbody == null)
        {
            return;
        }

        Vector2 velocity = ballRigidbody.linearVelocity;

        if (velocity.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        ballRigidbody.linearVelocity = velocity.normalized * currentSpeed;
    }

    public virtual void ResetState()
    {
        RestoreIgnoredCollision();
        wallHitCount = 0;
        state = BallState.Ready;
        hasExitedEndline = false;

        if (ballRigidbody != null)
        {
            ballRigidbody.linearVelocity = Vector2.zero;
            ballRigidbody.angularVelocity = 0f;
        }
    }

    public virtual void Launch(Vector2 direction, float speed)
    {
        BeginLaunch(direction, speed, false);
    }

    public virtual void LaunchFromPlayfield(Vector2 direction, float speed)
    {
        BeginLaunch(direction, speed, true);
    }

    private void BeginLaunch(Vector2 direction, float speed, bool startsOutsideEndline)
    {
        Vector2 normalizedDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.up;
        currentSpeed = speed > 0f ? speed : 12f;

        state = BallState.Flying;
        hasExitedEndline = startsOutsideEndline;
        ballRigidbody.linearVelocity = normalizedDirection * currentSpeed;
        OnLaunched();
    }

    public void SetReturnTarget(Transform target)
    {
        returnTarget = target;
    }

    public void IgnoreCollisionUntilSeparated(Collider2D otherCollider)
    {
        RestoreIgnoredCollision();

        if (ballCollider == null || otherCollider == null)
        {
            return;
        }

        ignoredCollider = otherCollider;
        Physics2D.IgnoreCollision(ballCollider, ignoredCollider, true);
    }

    public int ConsumeWallHitCount()
    {
        int count = wallHitCount;
        wallHitCount = 0;
        return count;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int collisionLayer = collision.gameObject.layer;

        if (collisionLayer == endlineLayer && state == BallState.Flying && hasExitedEndline)
        {
            state = BallState.Returning;
            return;
        }

        if (collisionLayer == shooterLayer && state == BallState.Returning)
        {
            ReturnRequested?.Invoke(this);
            return;
        }

        if (collisionLayer != enemyTriggerLayer || state != BallState.Flying)
        {
            return;
        }

        Enemy_Base enemy = collision.GetComponent<Enemy_Base>();

        if (enemy == null)
        {
            return;
        }

        RaiseEnemyHit(enemy, GetTriggerHitPoint(collision));
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == endlineLayer && state == BallState.Flying)
        {
            hasExitedEndline = true;
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int collisionLayer = collision.gameObject.layer;

        if (collisionLayer == wallLayer)
        {
            wallHitCount++;

            if (state == BallState.Returning)
            {
                RedirectToReturnTarget();
                return;
            }

            if (state == BallState.Flying)
            {
                OnFlyingCollisionResolved(collision);
            }

            return;
        }

        if (collisionLayer != enemySolidLayer || state != BallState.Flying)
        {
            return;
        }

        Enemy_Base enemy = collision.gameObject.GetComponentInParent<Enemy_Base>();

        if (enemy == null)
        {
            return;
        }

        RaiseEnemyHit(enemy, GetCollisionHitPoint(collision));
        OnFlyingCollisionResolved(collision);
    }

    protected virtual void OnLaunched()
    {
    }

    protected virtual void OnFlyingCollisionResolved(Collision2D collision)
    {
    }

    protected void SetFlightMotion(Vector2 direction, float speed)
    {
        if (ballRigidbody == null || direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        currentSpeed = Mathf.Max(0f, speed);
        ballRigidbody.linearVelocity = direction.normalized * currentSpeed;
    }

    private void RaiseEnemyHit(Enemy_Base enemy, Vector2 hitPoint)
    {
        if (enemy != null)
        {
            EnemyHit?.Invoke(this, enemy, hitPoint);
        }
    }

    private Vector2 GetCollisionHitPoint(Collision2D collision)
    {
        return collision.contactCount > 0
            ? collision.GetContact(0).point
            : (Vector2)transform.position;
    }

    private Vector2 GetTriggerHitPoint(Collider2D enemyCollider)
    {
        if (ballCollider == null || enemyCollider == null)
        {
            return transform.position;
        }

        ColliderDistance2D distance = ballCollider.Distance(enemyCollider);

        return distance.isValid
            ? distance.pointB
            : (Vector2)transform.position;
    }

    private void RedirectToReturnTarget()
    {
        if (returnTarget == null || ballRigidbody == null)
        {
            return;
        }

        Vector2 directionToTarget =
            ((Vector2)returnTarget.position - ballRigidbody.position).normalized;

        ballRigidbody.linearVelocity = directionToTarget * currentSpeed;
    }

    private void UpdateIgnoredCollision()
    {
        if (ignoredCollider == null || ballCollider == null)
        {
            ignoredCollider = null;
            return;
        }

        ColliderDistance2D distance = ballCollider.Distance(ignoredCollider);

        if (!distance.isValid || distance.distance > 0.001f)
        {
            RestoreIgnoredCollision();
        }
    }

    private void RestoreIgnoredCollision()
    {
        if (ballCollider != null && ignoredCollider != null)
        {
            Physics2D.IgnoreCollision(ballCollider, ignoredCollider, false);
        }

        ignoredCollider = null;
    }
}
