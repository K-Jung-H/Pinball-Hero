using UnityEngine;

public class CombatPipeline : MonoBehaviour
{
    [SerializeField] private int defaultBallDamage = 1;

    private readonly DamageCalculator damageCalculator = new DamageCalculator();

    public void RegisterBall(Ball_Base ball)
    {
        if (ball == null)
        {
            return;
        }

        ball.EnemyHit -= ProcessHit;
        ball.EnemyHit += ProcessHit;
    }

    public void UnregisterBall(Ball_Base ball)
    {
        if (ball == null)
        {
            return;
        }

        ball.EnemyHit -= ProcessHit;
    }

    private void ProcessHit(Ball_Base ball, Enemy_Base enemy)
    {
        if (ball == null || enemy == null)
        {
            return;
        }

        BallHitContext context = new BallHitContext(ball, enemy);
        BallHitResult result = damageCalculator.Calculate(context, defaultBallDamage);

        if (result.Damage > 0)
        {
            enemy.TakeDamage(result.Damage);
        }
    }
}

public readonly struct BallHitContext
{
    public BallHitContext(Ball_Base ball, Enemy_Base enemy)
    {
        Ball = ball;
        Enemy = enemy;
    }

    public Ball_Base Ball { get; }
    public Enemy_Base Enemy { get; }
}

public readonly struct BallHitResult
{
    public BallHitResult(int damage)
    {
        Damage = damage;
    }

    public int Damage { get; }
}

public sealed class DamageCalculator
{
    public BallHitResult Calculate(BallHitContext context, int baseDamage)
    {
        return new BallHitResult(Mathf.Max(0, baseDamage));
    }
}
