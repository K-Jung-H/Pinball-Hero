using UnityEngine;

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
        int damage = context.Ball != null ? context.Ball.CurrentHitDamage : 0;
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
