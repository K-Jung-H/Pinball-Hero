using System.Collections.Generic;
using UnityEngine;

public class BallShooter : MonoBehaviour
{
    [SerializeField] private BallFactory ballFactory;
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private int defaultBallCount = 4;
    [SerializeField] private float launchInterval = 0.3f;
    [SerializeField] private float launchSpeed = 20f;
    [SerializeField] private int clusterFragmentPoolSize = 8;

    private readonly Queue<Ball_Base> readyQueue = new Queue<Ball_Base>();
    private readonly Queue<Ball_Base> clusterFragmentPool = new Queue<Ball_Base>();
    private readonly Queue<ClusterFragmentSpawnRequest> pendingClusterFragments =
        new Queue<ClusterFragmentSpawnRequest>();

    private CombatPipeline combatPipeline;
    private StageBallProgress[] stageBallProgresses;
    private RunSkillInventory runSkillInventory;
    private Vector2 launchDirection = Vector2.up;
    private float launchTimer;

    public void SetCombatPipeline(CombatPipeline pipeline)
    {
        combatPipeline = pipeline;
    }

    public void SetRuntimeData(StageBallProgress[] progress, RunSkillInventory inventory)
    {
        stageBallProgresses = progress;
        runSkillInventory = inventory;
    }

    private void Start()
    {
        for (int i = 0; i < defaultBallCount; i++)
        {
            AddBall(BallType.Normal);
        }

        for (int i = 0; i < Mathf.Max(0, clusterFragmentPoolSize); i++)
        {
            Ball_Base fragment = CreateClusterFragment();

            if (fragment != null)
            {
                clusterFragmentPool.Enqueue(fragment);
            }
        }
    }

    private void FixedUpdate()
    {
        while (pendingClusterFragments.Count > 0)
        {
            SpawnClusterFragment(pendingClusterFragments.Dequeue());
        }
    }

    private void Update()
    {
        launchTimer += Time.deltaTime;

        if (launchTimer < launchInterval)
        {
            return;
        }

        launchTimer = 0f;
        TryLaunchNextBall();
    }

    public void SetLaunchDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        launchDirection = direction.normalized;
    }

    public void AddBall(BallType type)
    {
        if (ballFactory == null)
        {
            Debug.LogError("BallFactory is not assigned.");
            return;
        }

        Ball_Base ball = ballFactory.Create(type, spawnPoint.position, transform, stageBallProgresses, runSkillInventory);

        if (ball == null)
        {
            return;
        }

        ball.SetReturnTarget(transform);
        ball.ReturnRequested += ReturnBall;

        if (combatPipeline == null)
        {
            Debug.LogError("CombatPipeline is not initialized.");
        }
        else
        {
            combatPipeline.RegisterBall(ball);
        }

        ReturnBall(ball);
    }

    public void RequestClusterFragment(
        Ball_Base sourceBall,
        Collider2D sourceCollider,
        int damage,
        float minNoiseAngle,
        float maxNoiseAngle)
    {
        if (sourceBall == null || damage <= 0)
        {
            return;
        }

        Vector2 sourceVelocity = sourceBall.Velocity;

        pendingClusterFragments.Enqueue(new ClusterFragmentSpawnRequest(
            sourceBall,
            sourceCollider,
            sourceBall.transform.position,
            sourceVelocity,
            damage,
            minNoiseAngle,
            maxNoiseAngle));
    }

    private void TryLaunchNextBall()
    {
        if (readyQueue.Count <= 0)
        {
            return;
        }

        Ball_Base ball = readyQueue.Dequeue();

        ball.transform.position = spawnPoint.position;
        ball.gameObject.SetActive(true);
        ballFactory.FillRuntimeStat(ball.BallType, stageBallProgresses, runSkillInventory, ball.RuntimeStat);
        ball.ApplyRuntimeStat();

        float speed = ball.RuntimeStat.MoveSpeed > 0f
            ? ball.RuntimeStat.MoveSpeed
            : launchSpeed;

        ball.Launch(launchDirection, speed);
    }

    private void ReturnBall(Ball_Base ball)
    {
        ball.ResetState();
        ball.transform.position = spawnPoint.position;
        ball.gameObject.SetActive(false);

        readyQueue.Enqueue(ball);
    }

    private void SpawnClusterFragment(ClusterFragmentSpawnRequest request)
    {
        Ball_Base fragment = clusterFragmentPool.Count > 0
            ? clusterFragmentPool.Dequeue()
            : CreateClusterFragment();

        if (fragment == null)
        {
            return;
        }

        Vector2 sourceVelocity = request.SourceBall != null
            && request.SourceBall.gameObject.activeInHierarchy
                ? request.SourceBall.Velocity
                : request.FallbackVelocity;

        Vector2 lookDirection = sourceVelocity.sqrMagnitude > 0.0001f
            ? sourceVelocity.normalized
            : Vector2.up;

        float speed = sourceVelocity.magnitude > 0.0001f
            ? sourceVelocity.magnitude
            : launchSpeed;

        Vector2 spawnPosition = request.SourceBall != null
            && request.SourceBall.gameObject.activeInHierarchy
                ? request.SourceBall.transform.position
                : request.FallbackPosition;

        Vector2 launchDirection = AddSideNoise(
            lookDirection,
            request.MinNoiseAngle,
            request.MaxNoiseAngle);

        fragment.ResetState();
        fragment.transform.position = spawnPosition;
        ballFactory.FillSpawnedBallRuntimeStat(
            BallType.ClusterFragment,
            request.Damage,
            fragment.RuntimeStat);
        fragment.ApplyRuntimeStat();
        fragment.gameObject.SetActive(true);

        if (request.SourceCollider != null
            && request.SourceCollider.enabled
            && request.SourceCollider.gameObject.activeInHierarchy)
        {
            fragment.IgnoreCollisionUntilSeparated(request.SourceCollider);
        }

        fragment.LaunchFromPlayfield(launchDirection, speed);
    }

    private Ball_Base CreateClusterFragment()
    {
        if (ballFactory == null || spawnPoint == null)
        {
            return null;
        }

        Ball_Base fragment = ballFactory.Create(
            BallType.ClusterFragment,
            spawnPoint.position,
            transform);

        if (fragment == null)
        {
            return null;
        }

        fragment.SetReturnTarget(transform);
        fragment.ReturnRequested -= ReleaseClusterFragment;
        fragment.ReturnRequested += ReleaseClusterFragment;

        if (combatPipeline != null)
        {
            combatPipeline.RegisterBall(fragment);
        }

        fragment.ResetState();
        fragment.gameObject.SetActive(false);

        return fragment;
    }

    private void ReleaseClusterFragment(Ball_Base fragment)
    {
        if (fragment == null)
        {
            return;
        }

        fragment.ResetState();
        fragment.transform.position = spawnPoint.position;
        fragment.gameObject.SetActive(false);
        clusterFragmentPool.Enqueue(fragment);
    }

    private static Vector2 AddSideNoise(
        Vector2 lookDirection,
        float minNoiseAngle,
        float maxNoiseAngle)
    {
        float safeMaxAngle = Mathf.Clamp(maxNoiseAngle, 0f, 89f);
        float safeMinAngle = Mathf.Clamp(minNoiseAngle, 0f, safeMaxAngle);
        float noiseAngle = Random.Range(safeMinAngle, safeMaxAngle);

        if (Random.value < 0.5f)
        {
            noiseAngle = -noiseAngle;
        }

        float radians = noiseAngle * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            lookDirection.x * cos - lookDirection.y * sin,
            lookDirection.x * sin + lookDirection.y * cos).normalized;
    }

    private readonly struct ClusterFragmentSpawnRequest
    {
        public ClusterFragmentSpawnRequest(
            Ball_Base sourceBall,
            Collider2D sourceCollider,
            Vector2 fallbackPosition,
            Vector2 fallbackVelocity,
            int damage,
            float minNoiseAngle,
            float maxNoiseAngle)
        {
            SourceBall = sourceBall;
            SourceCollider = sourceCollider;
            FallbackPosition = fallbackPosition;
            FallbackVelocity = fallbackVelocity;
            Damage = damage;
            MaxNoiseAngle = Mathf.Clamp(maxNoiseAngle, 0f, 89f);
            MinNoiseAngle = Mathf.Clamp(minNoiseAngle, 0f, MaxNoiseAngle);
        }

        public Ball_Base SourceBall { get; }
        public Collider2D SourceCollider { get; }
        public Vector2 FallbackPosition { get; }
        public Vector2 FallbackVelocity { get; }
        public int Damage { get; }
        public float MinNoiseAngle { get; }
        public float MaxNoiseAngle { get; }
    }
}
