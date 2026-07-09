using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BallShooter : MonoBehaviour
{
    [SerializeField] private BallFactorySO ballFactory;
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private int defaultBallCount = 4;
    [SerializeField] private float launchInterval = 0.3f;
    [SerializeField] private float launchSpeed = 20f;

    private readonly Queue<Ball_Base> readyQueue = new Queue<Ball_Base>();

    private CombatPipeline combatPipeline;
    private Vector2 launchDirection = Vector2.up;
    private float launchTimer;

    public void SetCombatPipeline(CombatPipeline pipeline)
    {
        combatPipeline = pipeline;
    }

    private void Start()
    {
        for (int i = 0; i < defaultBallCount; i++)
        {
            AddBall(BallType.Normal);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
        {
            AddBall(BallType.Fire);
        }

        
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
        Ball_Base ball = ballFactory.Create(type, spawnPoint.position, transform);

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

    private void TryLaunchNextBall()
    {
        if (readyQueue.Count <= 0)
        {
            return;
        }

        Ball_Base ball = readyQueue.Dequeue();

        ball.transform.position = spawnPoint.position;
        ball.gameObject.SetActive(true);

        ball.Launch(launchDirection, launchSpeed);
    }

    private void ReturnBall(Ball_Base ball)
    {
        ball.ResetState();
        ball.transform.position = spawnPoint.position;
        ball.gameObject.SetActive(false);

        readyQueue.Enqueue(ball);
    }
}
