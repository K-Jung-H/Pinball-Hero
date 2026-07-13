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

public readonly struct BallHitContext
{
    public BallHitContext(
        Ball_Base ball,
        Enemy_Base enemy,
        Vector2 hitPoint,
        int wallHitCount)
    {
        Ball = ball;
        Enemy = enemy;
        HitPoint = hitPoint;
        TargetCenter = enemy != null ? enemy.Center : hitPoint;
        HitSide = enemy != null
            ? enemy.GetHitSide(hitPoint)
            : EnemyHitSide.None;
        WallHitCount = Mathf.Max(0, wallHitCount);
    }

    public Ball_Base Ball { get; }
    public Enemy_Base Enemy { get; }
    public Vector2 HitPoint { get; }
    public Vector2 TargetCenter { get; }
    public EnemyHitSide HitSide { get; }
    public int WallHitCount { get; }
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
        Enemy_Base enemy,
        EnemyHitSide hitSide = EnemyHitSide.None,
        int wallHitCount = 0)
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
        HitSide = hitSide;
        WallHitCount = Mathf.Max(0, wallHitCount);
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
    public EnemyHitSide HitSide { get; }
    public int WallHitCount { get; }

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
            context.Enemy,
            context.HitSide,
            context.WallHitCount);
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
        float damageBonusRatio = 0f;
        float critChance = request.RuntimeStat != null
            ? request.RuntimeStat.CritChance
            : 0f;
        bool isCritical = false;

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

                damageBonusRatio += Mathf.Max(
                    0f,
                    passiveDefinition.GetDamageBonusRatio(state.Level, request));

                critChance += Mathf.Max(
                    0f,
                    passiveDefinition.GetCritChanceBonus(state.Level, request));
            }
        }

        damage = Mathf.Max(
            0,
            Mathf.RoundToInt(damage * (1f + damageBonusRatio)));

        if (request.CanCrit
            && request.RuntimeStat != null
            && UnityEngine.Random.value < Mathf.Clamp01(critChance))
        {
            damage = Mathf.RoundToInt(
                damage * request.RuntimeStat.CritDamageMultiplier);
            isCritical = true;
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
