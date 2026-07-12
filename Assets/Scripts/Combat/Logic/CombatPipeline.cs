using System;
using UnityEngine;

public class CombatPipeline : MonoBehaviour
{
    private readonly PassiveModifierSystem passiveModifierSystem = new PassiveModifierSystem();

    public event Action<DamageResult> HitResolved;

    public void SetRunSkillInventory(RunSkillInventory inventory)
    {
        passiveModifierSystem.SetRunSkillInventory(inventory);
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

        BallHitContext context = new BallHitContext(ball, enemy, hitPoint);
        DamageRequest request = DamageRequest.CreateDirectHit(context);
        DamageResult result = passiveModifierSystem.Apply(request);

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

public readonly struct DamageRequest
{
    public DamageRequest(
        int baseDamage,
        BallType ballType,
        DamageType damageType,
        DamageSourceType sourceType,
        bool canCrit,
        bool isDot,
        Vector2 hitPoint,
        BallRuntimeStat runtimeStat,
        Ball_Base ball,
        Enemy_Base enemy)
    {
        BaseDamage = baseDamage;
        BallType = ballType;
        DamageType = damageType;
        SourceType = sourceType;
        CanCrit = canCrit;
        IsDot = isDot;
        HitPoint = hitPoint;
        RuntimeStat = runtimeStat;
        Ball = ball;
        Enemy = enemy;
    }

    public int BaseDamage { get; }
    public BallType BallType { get; }
    public DamageType DamageType { get; }
    public DamageSourceType SourceType { get; }
    public bool CanCrit { get; }
    public bool IsDot { get; }
    public Vector2 HitPoint { get; }
    public BallRuntimeStat RuntimeStat { get; }
    public Ball_Base Ball { get; }
    public Enemy_Base Enemy { get; }

    public static DamageRequest CreateDirectHit(BallHitContext context)
    {
        BallRuntimeStat runtimeStat = context.Ball != null ? context.Ball.RuntimeStat : null;
        int damage = runtimeStat != null ? runtimeStat.HitDamage : 0;
        DamageType damageType = runtimeStat != null ? runtimeStat.DamageType : DamageType.Normal;

        return new DamageRequest(
            damage,
            context.BallType,
            damageType,
            DamageSourceType.DirectHit,
            true,
            false,
            context.HitPoint,
            runtimeStat,
            context.Ball,
            context.Enemy);
    }
}

public readonly struct DamageResult
{
    public DamageResult(
        int damage,
        DamageType damageType,
        DamageSourceType sourceType,
        bool isCritical,
        bool isDot,
        Vector2 hitPoint,
        Ball_Base ball,
        Enemy_Base enemy)
    {
        Damage = damage;
        DamageType = damageType;
        SourceType = sourceType;
        IsCritical = isCritical;
        IsDot = isDot;
        HitPoint = hitPoint;
        Ball = ball;
        Enemy = enemy;
    }

    public int Damage { get; }
    public DamageType DamageType { get; }
    public DamageSourceType SourceType { get; }
    public bool IsCritical { get; }
    public bool IsDot { get; }
    public Vector2 HitPoint { get; }
    public Ball_Base Ball { get; }
    public Enemy_Base Enemy { get; }
}

public sealed class PassiveModifierSystem
{
    private RunSkillInventory runSkillInventory;

    public void SetRunSkillInventory(RunSkillInventory inventory)
    {
        runSkillInventory = inventory;
    }

    public DamageResult Apply(DamageRequest request)
    {
        int damage = Mathf.Max(0, request.BaseDamage);
        bool isCritical = false;

        if (request.CanCrit && request.RuntimeStat != null && request.RuntimeStat.CritChance > 0f)
        {
            if (UnityEngine.Random.value <= request.RuntimeStat.CritChance)
            {
                damage = Mathf.RoundToInt(damage * request.RuntimeStat.CritDamageMultiplier);
                isCritical = true;
            }
        }

        if (runSkillInventory != null)
        {
            for (int i = 0; i < runSkillInventory.PassiveSkillCount; i++)
            {
                RunSkillState state = runSkillInventory.GetPassiveSkill(i);

                PassiveSkillDefinitionSO passiveDefinition = state != null
                    ? state.Definition as PassiveSkillDefinitionSO
                    : null;

                if (passiveDefinition == null)
                {
                    continue;
                }

                if (!passiveDefinition.TryGetLevelData(state.Level, out PassiveSkillLevelData levelData))
                {
                    continue;
                }

                if (!levelData.Matches(request))
                {
                    continue;
                }

                damage = Mathf.Max(0, Mathf.RoundToInt((damage + levelData.DamageAdd) * levelData.DamageMultiplier));
            }
        }

        return new DamageResult(
            damage,
            request.DamageType,
            request.SourceType,
            isCritical,
            request.IsDot,
            request.HitPoint,
            request.Ball,
            request.Enemy);
    }
}

public enum DamageType
{
    Normal = 0,
    Fire = 1,
    Ice = 2,
    Laser = 3,
    Explosion = 4
}
