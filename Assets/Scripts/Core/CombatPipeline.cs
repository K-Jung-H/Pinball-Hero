using System;
using UnityEngine;

public class CombatPipeline : MonoBehaviour
{
    [SerializeField] private int defaultBallDamage = 1;

    private readonly DamageCalculator damageCalculator = new DamageCalculator();
    private readonly DamageModifierSystem damageModifierSystem = new DamageModifierSystem();

    public event Action<BallHitResult> HitResolved;

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

    private void ProcessHit(Ball_Base ball, Enemy_Base enemy, Vector2 hitPoint)
    {
        if (ball == null || enemy == null)
        {
            return;
        }

        BallHitContext context = new BallHitContext(ball, enemy, hitPoint);
        DamageDraft damageDraft = damageCalculator.Calculate(context, defaultBallDamage);
        BallHitResult result = damageModifierSystem.Apply(context, damageDraft);

        if (result.Damage > 0)
        {
            enemy.TakeDamage(result.Damage);
            HitResolved?.Invoke(result);
        }
    }
}

public readonly struct BallHitContext
{
    public BallHitContext(Ball_Base ball, Enemy_Base enemy, Vector2 hitPoint)
    {
        Ball = ball;
        Enemy = enemy;
        HitPoint = hitPoint;
    }

    public Ball_Base Ball { get; }
    public Enemy_Base Enemy { get; }
    public Vector2 HitPoint { get; }
    public BallType BallType => Ball != null ? Ball.BallType : BallType.None;
}

public readonly struct BallHitResult
{
    public BallHitResult(
        int damage,
        DamageType damageType,
        bool isCritical,
        bool isDot,
        Vector2 hitPoint,
        Ball_Base ball,
        Enemy_Base enemy)
    {
        Damage = damage;
        DamageType = damageType;
        IsCritical = isCritical;
        IsDot = isDot;
        HitPoint = hitPoint;
        Ball = ball;
        Enemy = enemy;
    }

    public int Damage { get; }
    public DamageType DamageType { get; }
    public bool IsCritical { get; }
    public bool IsDot { get; }
    public Vector2 HitPoint { get; }
    public Ball_Base Ball { get; }
    public Enemy_Base Enemy { get; }
}

public sealed class DamageCalculator
{
    public DamageDraft Calculate(BallHitContext context, int baseDamage)
    {
        return new DamageDraft(Mathf.Max(0, baseDamage), DamageType.Normal);
    }
}

public sealed class DamageModifierSystem
{
    private IDamageModifier[] modifiers = Array.Empty<IDamageModifier>();

    public void SetModifiers(IDamageModifier[] value)
    {
        modifiers = value ?? Array.Empty<IDamageModifier>();
    }

    public BallHitResult Apply(BallHitContext context, DamageDraft damageDraft)
    {
        for (int i = 0; i < modifiers.Length; i++)
        {
            modifiers[i]?.Modify(context, ref damageDraft);
        }

        return new BallHitResult(
            Mathf.Max(0, damageDraft.Damage),
            damageDraft.DamageType,
            damageDraft.IsCritical,
            damageDraft.IsDot,
            context.HitPoint,
            context.Ball,
            context.Enemy);
    }
}

public interface IDamageModifier
{
    void Modify(BallHitContext context, ref DamageDraft damageDraft);
}

public enum DamageType
{
    Normal = 0,
    Fire = 1,
    Ice = 2,
    Laser = 3,
    Explosion = 4
}

public struct DamageDraft
{
    public DamageDraft(int damage, DamageType damageType)
    {
        Damage = damage;
        DamageType = damageType;
        IsCritical = false;
        IsDot = false;
    }

    public int Damage;
    public DamageType DamageType;
    public bool IsCritical;
    public bool IsDot;
}
