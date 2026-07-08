using UnityEngine;

public class Ball_Base : MonoBehaviour
{
    [SerializeField] private BallType ballType = BallType.Normal;
    [SerializeField] private Rigidbody2D ballRigidbody;
    [SerializeField] private Collider2D ballCollider;
    
    private bool isLaunched;
    private int wallHitCount;
    private Vector2 lastVelocity;



    public BallType BallType => ballType;
    public int WallHitCount => wallHitCount;
    public Vector2 Velocity => ballRigidbody != null ? ballRigidbody.linearVelocity : Vector2.zero;

    protected void Awake()
    {
        if (ballRigidbody == null)
        {
            ballRigidbody = GetComponent<Rigidbody2D>();
        }

        if (ballCollider == null)
        {
            ballCollider = GetComponent<Collider2D>();
        }
    }

    
    public virtual void ResetState()
    {
        wallHitCount = 0;
        isLaunched = false;
        lastVelocity = Vector2.zero;

        if (ballRigidbody != null)
        {
            ballRigidbody.linearVelocity = Vector2.zero;
            ballRigidbody.angularVelocity = 0f;
        }
    }

    public virtual void Launch(Vector2 direction, float speed)
    {
        Vector2 normalizedDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.up;
        float launchSpeed = speed > 0f ? speed : 12f;

        isLaunched = true;
        ballRigidbody.linearVelocity = normalizedDirection * launchSpeed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        
    }
}
