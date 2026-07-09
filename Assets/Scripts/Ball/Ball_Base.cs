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

    [Header("Definition")]
    [SerializeField] private BallType ballType = BallType.Normal;

    [Header("Components")]
    [SerializeField] private Rigidbody2D ballRigidbody;
    [SerializeField] private Collider2D ballCollider;

    [Header("Return")]
    [SerializeField] private Transform returnTarget;

    private static int endlineLayer = -1;
    private static int shooterLayer = -1;
    private static int enemyLayer = -1;

    private float currentSpeed;
    private BallState state;
    private bool hasExitedEndline;
    private int wallHitCount;

    public event Action<Ball_Base> ReturnRequested;
    public event Action<Ball_Base, Enemy_Base> EnemyHit;

    public BallType BallType => ballType;
    public int WallHitCount => wallHitCount;
    public Vector2 Velocity => ballRigidbody != null ? ballRigidbody.linearVelocity : Vector2.zero;

    protected void Awake()
    {
        CacheLayers();

        if (ballRigidbody == null)
        {
            ballRigidbody = GetComponent<Rigidbody2D>();
        }

        if (ballCollider == null)
        {
            ballCollider = GetComponent<Collider2D>();
        }
    }

    private static void CacheLayers()
    {
        if (endlineLayer != -1 && shooterLayer != -1 && enemyLayer != -1)
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

        if (enemyLayer == -1)
        {
            enemyLayer = LayerMask.NameToLayer("Enemy");
        }

        if (endlineLayer == -1)
        {
            Debug.LogError("Endline layer is not defined.");
        }

        if (shooterLayer == -1)
        {
            Debug.LogError("Shooter layer is not defined.");
        }

        if (enemyLayer == -1)
        {
            Debug.LogError("Enemy layer is not defined.");
        }
    }

    private void FixedUpdate()
    {
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
        Vector2 normalizedDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.up;
        currentSpeed = speed > 0f ? speed : 12f;

        state = BallState.Flying;
        hasExitedEndline = false;
        ballRigidbody.linearVelocity = normalizedDirection * currentSpeed;
    }

    public void SetReturnTarget(Transform target)
    {
        returnTarget = target;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == endlineLayer && state == BallState.Flying && hasExitedEndline)
        {
            state = BallState.Returning;
            return;
        }

        if (collision.gameObject.layer == shooterLayer && state == BallState.Returning)
        {
            ReturnRequested?.Invoke(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.layer == endlineLayer && state == BallState.Flying)
        {
            hasExitedEndline = true;
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        wallHitCount++;

        if (state == BallState.Flying && collision.gameObject.layer == enemyLayer)
        {
            Enemy_Base enemy = collision.gameObject.GetComponent<Enemy_Base>();

            if (enemy != null)
            {
                EnemyHit?.Invoke(this, enemy);
            }
        }

        if (state == BallState.Returning)
        {
            RedirectToReturnTarget();
        }
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
}
