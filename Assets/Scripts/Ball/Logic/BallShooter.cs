using System.Collections.Generic;
using UnityEngine;

public class BallShooter : MonoBehaviour
{
    [SerializeField] private BallFactory ballFactory;
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private int defaultBallCount = 4;
    [SerializeField] private float launchInterval = 0.3f;
    [SerializeField] private float launchSpeed = 20f;

    private readonly Queue<Ball_Base> readyQueue = new Queue<Ball_Base>();

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
}
