using UnityEngine;

public sealed class HitEffectSystem
{
    private readonly BallHitEffectHandler[] handlers =
        new BallHitEffectHandler[(int)BallEffectType.Cluster + 1];
    private readonly CombatPipeline combatPipeline;

    public HitEffectSystem(CombatPipeline pipeline)
    {
        combatPipeline = pipeline;

        Register(new BurnHitEffectHandler(pipeline));
        Register(new FreezeHitEffectHandler());
    }

    public void SetAreaEffectSystem(AreaEffectSystem areaEffectSystem)
    {
        int handlerIndex = (int)BallEffectType.Laser;

        handlers[handlerIndex] = areaEffectSystem != null
            ? new LaserHitEffectHandler(combatPipeline, areaEffectSystem)
            : null;
    }

    public void SetBallShooter(BallShooter ballShooter)
    {
        int handlerIndex = (int)BallEffectType.Cluster;

        handlers[handlerIndex] = ballShooter != null
            ? new ClusterHitEffectHandler(ballShooter)
            : null;
    }

    public void Execute(BallHitContext context)
    {
        BallRuntimeStat runtimeStat = context.Ball != null
            ? context.Ball.RuntimeStat
            : null;

        if (runtimeStat == null)
        {
            return;
        }

        for (int i = 0; i < runtimeStat.EffectCount; i++)
        {
            BallEffectRuntimeStat effect = runtimeStat.Effects[i];

            if (effect == null)
            {
                continue;
            }

            int handlerIndex = (int)effect.EffectType;

            if (handlerIndex < 0 || handlerIndex >= handlers.Length)
            {
                continue;
            }

            handlers[handlerIndex]?.Execute(context, effect);
        }
    }

    private void Register(BallHitEffectHandler handler)
    {
        int handlerIndex = (int)handler.EffectType;

        if (handlerIndex < 0 || handlerIndex >= handlers.Length)
        {
            return;
        }

        handlers[handlerIndex] = handler;
    }
}

public abstract class BallHitEffectHandler
{
    public abstract BallEffectType EffectType { get; }
    public abstract void Execute(BallHitContext context, BallEffectRuntimeStat effect);
}

public abstract class BallHitEffectHandler<T> : BallHitEffectHandler
    where T : BallEffectRuntimeStat
{
    public sealed override void Execute(BallHitContext context, BallEffectRuntimeStat effect)
    {
        if (effect is T typedEffect)
        {
            Execute(context, typedEffect);
        }
    }

    protected abstract void Execute(BallHitContext context, T effect);
}

public sealed class BurnHitEffectHandler : BallHitEffectHandler<BurnEffectRuntimeStat>
{
    private readonly CombatPipeline combatPipeline;

    public BurnHitEffectHandler(CombatPipeline pipeline)
    {
        combatPipeline = pipeline;
    }

    public override BallEffectType EffectType => BallEffectType.Burn;

    protected override void Execute(BallHitContext context, BurnEffectRuntimeStat effect)
    {
        if (context.Enemy == null || !context.Enemy.CanReceiveDamage)
        {
            return;
        }

        StatusController statusController = context.Enemy.StatusController;

        if (statusController != null)
        {
            statusController.ApplyBurn(
                effect.DotDamage,
                effect.Duration,
                effect.MaxStack,
                combatPipeline);
        }
    }
}

public sealed class FreezeHitEffectHandler : BallHitEffectHandler<FreezeEffectRuntimeStat>
{
    public override BallEffectType EffectType => BallEffectType.Freeze;

    protected override void Execute(BallHitContext context, FreezeEffectRuntimeStat effect)
    {
        if (context.Enemy == null
            || !context.Enemy.CanReceiveDamage
            || effect.Chance <= 0f
            || Random.value > effect.Chance)
        {
            return;
        }

        StatusController statusController = context.Enemy.StatusController;

        if (statusController != null)
        {
            statusController.ApplyFreeze(
                effect.Duration,
                effect.SlowRatio,
                effect.ExtraDamageRatio);
        }
    }
}

public sealed class LaserHitEffectHandler : BallHitEffectHandler<LaserEffectRuntimeStat>
{
    private readonly CombatPipeline combatPipeline;
    private readonly AreaEffectSystem areaEffectSystem;

    public LaserHitEffectHandler(
        CombatPipeline pipeline,
        AreaEffectSystem effectSystem)
    {
        combatPipeline = pipeline;
        areaEffectSystem = effectSystem;
    }

    public override BallEffectType EffectType => BallEffectType.Laser;

    protected override void Execute(BallHitContext context, LaserEffectRuntimeStat effect)
    {
        Bounds boardBounds = areaEffectSystem.BoardBounds;
        float areaWidth = boardBounds.size.x;
        float areaHeight = areaEffectSystem.CellSize.y * effect.RowHeightInCells;

        if (effect.AreaPrefab == null
            || effect.RowDamage <= 0
            || areaWidth <= 0f
            || areaHeight <= 0f)
        {
            return;
        }

        Vector2 center = new Vector2(boardBounds.center.x, context.TargetCenter.y);
        Vector2 size = new Vector2(areaWidth, areaHeight);

        areaEffectSystem.Play(
            effect.AreaPrefab,
            combatPipeline,
            center,
            size,
            effect.RowDamage,
            context.BallType,
            DamageType.Laser);
    }
}


public sealed class ClusterHitEffectHandler : BallHitEffectHandler<ClusterEffectRuntimeStat>
{
    private readonly BallShooter ballShooter;

    public ClusterHitEffectHandler(BallShooter shooter)
    {
        ballShooter = shooter;
    }

    public override BallEffectType EffectType => BallEffectType.Cluster;

    protected override void Execute(BallHitContext context, ClusterEffectRuntimeStat effect)
    {
        if (context.Ball == null
            || effect.SpawnedBallType != BallType.ClusterFragment
            || effect.SpawnDamage <= 0
            || effect.SpawnChance <= 0f)
        {
            return;
        }

        if (effect.SpawnChance < 1f && Random.value >= effect.SpawnChance)
        {
            return;
        }

        Collider2D sourceCollider = context.Enemy != null
            ? context.Enemy.BodyCollider
            : null;

        ballShooter.RequestClusterFragment(
            context.Ball,
            sourceCollider,
            effect.SpawnDamage,
            effect.MinNoiseAngle,
            effect.MaxNoiseAngle);
    }
}
