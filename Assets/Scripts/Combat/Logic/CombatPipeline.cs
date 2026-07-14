using System;
using UnityEngine;

public class CombatPipeline : MonoBehaviour
{
    private readonly PassiveModifierSystem passiveModifierSystem = new PassiveModifierSystem();
    private HitEffectSystem hitEffectSystem;

    public event Action<DamageResult> HitResolved;

    private void Awake()
    {
        EnsureHitEffectSystem();
    }

    public void SetRunSkillInventory(RunSkillInventory inventory)
    {
        passiveModifierSystem.SetRunSkillInventory(inventory);
    }

    public void SetAreaEffectSystem(AreaEffectSystem areaEffectSystem)
    {
        EnsureHitEffectSystem();
        hitEffectSystem.SetAreaEffectSystem(areaEffectSystem);
    }

    public void SetBallShooter(BallShooter ballShooter)
    {
        EnsureHitEffectSystem();
        hitEffectSystem.SetBallShooter(ballShooter);
    }

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

        int wallHitCount = ball.ConsumeWallHitCount();
        BallHitContext context = new BallHitContext(
            ball,
            enemy,
            hitPoint,
            wallHitCount);
        DamageRequest request = DamageRequest.CreateDirectHit(context);
        DamageResult result = ResolveDamage(request);

        if (result.Damage > 0)
        {
            EnsureHitEffectSystem();
            hitEffectSystem.Execute(context);
        }
    }

    private void EnsureHitEffectSystem()
    {
        if (hitEffectSystem == null)
        {
            hitEffectSystem = new HitEffectSystem(this);
        }
    }

    public DamageResult ResolveDamage(DamageRequest request)
    {
        Enemy_Base enemy = request.Enemy;

        if (enemy == null || !enemy.CanReceiveDamage)
        {
            return default;
        }

        DamageResult result = passiveModifierSystem.Apply(request);
        float incomingDamageMultiplier = enemy.StatusController != null
            ? enemy.StatusController.IncomingDamageMultiplier
            : 1f;

        int finalDamage = Mathf.Max(0, Mathf.RoundToInt(result.Damage * incomingDamageMultiplier));

        if (finalDamage <= 0)
        {
            return default;
        }

        result = new DamageResult(
            finalDamage,
            result.DamageType,
            result.SourceType,
            result.IsCritical,
            result.IsDot,
            result.HitPoint,
            result.Ball,
            result.Enemy);

        enemy.TakeDamage(finalDamage);
        HitResolved?.Invoke(result);

        return result;
    }
}
