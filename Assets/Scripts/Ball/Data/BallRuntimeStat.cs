using UnityEngine;

public sealed class BallRuntimeStat
{
    private BallEffectRuntimeStat[] effects = new BallEffectRuntimeStat[0];

    public BallType BallType { get; private set; } = BallType.None;
    public DamageType DamageType { get; private set; } = DamageType.Normal;
    public int HitDamage { get; private set; }
    public float MoveSpeed { get; private set; }
    public float CritChance { get; private set; }
    public float CritDamageMultiplier { get; private set; } = 1f;
    public BallEffectRuntimeStat[] Effects => effects;
    public int EffectCount { get; private set; }

    public void SetBase(BallDefinitionSO definition)
    {
        if (definition == null)
        {
            Clear();
            return;
        }

        BallType = definition.BallType;
        DamageType = definition.DamageType;
        HitDamage = Mathf.Max(0, definition.BaseHitDamage);
        MoveSpeed = Mathf.Max(0f, definition.BaseMoveSpeed);
        CritChance = Mathf.Clamp01(definition.BaseCritChance);
        CritDamageMultiplier = Mathf.Max(1f, definition.BaseCritDamageMultiplier);

        BallEffectDefinitionSO[] definitions = definition.Effects;
        int count = definitions != null ? definitions.Length : 0;
        EnsureEffectCapacity(count);
        EffectCount = count;

        for (int i = 0; i < count; i++)
        {
            BallEffectDefinitionSO effectDefinition = definitions[i];

            if (effectDefinition == null)
            {
                effects[i] = null;
                continue;
            }

            if (effects[i] == null || effects[i].EffectType != effectDefinition.EffectType)
            {
                effects[i] = effectDefinition.CreateRuntimeStat();
            }
            else
            {
                effects[i].ResetFrom(effectDefinition);
            }
        }
    }

    public void ApplyGrowthLevel(BallGrowthLevelData levelData)
    {
        if (levelData == null)
        {
            return;
        }

        ApplyHitDamageModifier(levelData.HitDamageAdd, levelData.HitDamageMultiplier);
        ApplyMoveSpeedMultiplier(levelData.MoveSpeedMultiplier);
    }

    public void ApplyHitDamageModifier(int add, float multiplier)
    {
        float safeMultiplier = multiplier <= 0f ? 1f : multiplier;
        HitDamage = Mathf.Max(0, Mathf.RoundToInt((HitDamage + add) * safeMultiplier));
    }

    public void ApplyMoveSpeedMultiplier(float multiplier)
    {
        float safeMultiplier = multiplier <= 0f ? 1f : multiplier;
        MoveSpeed = Mathf.Max(0f, MoveSpeed * safeMultiplier);
    }

    public T GetEffect<T>() where T : BallEffectRuntimeStat
    {
        for (int i = 0; i < EffectCount; i++)
        {
            if (effects[i] is T typedEffect)
            {
                return typedEffect;
            }
        }

        return null;
    }

    private void EnsureEffectCapacity(int count)
    {
        if (effects != null && effects.Length >= count)
        {
            return;
        }

        BallEffectRuntimeStat[] newEffects = new BallEffectRuntimeStat[count];

        if (effects != null)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                newEffects[i] = effects[i];
            }
        }

        effects = newEffects;
    }

    private void Clear()
    {
        BallType = BallType.None;
        DamageType = DamageType.Normal;
        HitDamage = 0;
        MoveSpeed = 0f;
        CritChance = 0f;
        CritDamageMultiplier = 1f;
        EffectCount = 0;
    }
}

public abstract class BallEffectRuntimeStat
{
    protected BallEffectRuntimeStat(BallEffectType effectType)
    {
        EffectType = effectType;
    }

    public BallEffectType EffectType { get; }

    public abstract void ResetFrom(BallEffectDefinitionSO definition);
}

public sealed class BurnEffectRuntimeStat : BallEffectRuntimeStat
{
    public BurnEffectRuntimeStat(BurnEffectDefinitionSO definition) : base(BallEffectType.Burn)
    {
        ResetFrom(definition);
    }

    public int DotDamage { get; private set; }
    public float Duration { get; private set; }
    public int MaxStack { get; private set; }

    public override void ResetFrom(BallEffectDefinitionSO definition)
    {
        BurnEffectDefinitionSO burn = definition as BurnEffectDefinitionSO;

        DotDamage = burn != null ? Mathf.Max(0, burn.BaseDotDamage) : 0;
        Duration = burn != null ? Mathf.Max(0f, burn.BaseDuration) : 0f;
        MaxStack = burn != null ? Mathf.Max(0, burn.BaseMaxStack) : 0;
    }

    public void ApplyLevel(int dotDamage, float duration, int maxStack)
    {
        DotDamage = Mathf.Max(0, dotDamage);
        Duration = Mathf.Max(0f, duration);
        MaxStack = Mathf.Max(0, maxStack);
    }
}

public sealed class FreezeEffectRuntimeStat : BallEffectRuntimeStat
{
    public FreezeEffectRuntimeStat(FreezeEffectDefinitionSO definition) : base(BallEffectType.Freeze)
    {
        ResetFrom(definition);
    }

    public float Chance { get; private set; }
    public float Duration { get; private set; }
    public float SlowRatio { get; private set; }
    public float ExtraDamageRatio { get; private set; }

    public override void ResetFrom(BallEffectDefinitionSO definition)
    {
        FreezeEffectDefinitionSO freeze = definition as FreezeEffectDefinitionSO;

        Chance = freeze != null ? Mathf.Clamp01(freeze.BaseChance) : 0f;
        Duration = freeze != null ? Mathf.Max(0f, freeze.BaseDuration) : 0f;
        SlowRatio = freeze != null ? Mathf.Max(0f, freeze.BaseSlowRatio) : 0f;
        ExtraDamageRatio = freeze != null ? Mathf.Max(0f, freeze.BaseExtraDamageRatio) : 0f;
    }

    public void ApplyLevel(float chance, float duration, float slowRatio, float extraDamageRatio)
    {
        Chance = Mathf.Clamp01(chance);
        Duration = Mathf.Max(0f, duration);
        SlowRatio = Mathf.Max(0f, slowRatio);
        ExtraDamageRatio = Mathf.Max(0f, extraDamageRatio);
    }
}

public sealed class LaserEffectRuntimeStat : BallEffectRuntimeStat
{
    public LaserEffectRuntimeStat(LaserEffectDefinitionSO definition) : base(BallEffectType.Laser)
    {
        ResetFrom(definition);
    }

    public int RowDamage { get; private set; }

    public override void ResetFrom(BallEffectDefinitionSO definition)
    {
        LaserEffectDefinitionSO laser = definition as LaserEffectDefinitionSO;
        RowDamage = laser != null ? Mathf.Max(0, laser.BaseRowDamage) : 0;
    }

    public void ApplyLevel(int rowDamage)
    {
        RowDamage = Mathf.Max(0, rowDamage);
    }
}

public sealed class PierceEffectRuntimeStat : BallEffectRuntimeStat
{
    public PierceEffectRuntimeStat(PierceEffectDefinitionSO definition) : base(BallEffectType.Pierce)
    {
        ResetFrom(definition);
    }

    public override void ResetFrom(BallEffectDefinitionSO definition)
    {
    }
}

public sealed class ClusterEffectRuntimeStat : BallEffectRuntimeStat
{
    public ClusterEffectRuntimeStat(ClusterEffectDefinitionSO definition) : base(BallEffectType.Cluster)
    {
        ResetFrom(definition);
    }

    public float SpawnChance { get; private set; }
    public int SpawnDamage { get; private set; }
    public BallType SpawnedBallType { get; private set; }

    public override void ResetFrom(BallEffectDefinitionSO definition)
    {
        ClusterEffectDefinitionSO cluster = definition as ClusterEffectDefinitionSO;

        SpawnChance = cluster != null ? Mathf.Clamp01(cluster.BaseSpawnChance) : 0f;
        SpawnDamage = cluster != null ? Mathf.Max(0, cluster.BaseSpawnDamage) : 0;
        SpawnedBallType = cluster != null ? cluster.SpawnedBallType : BallType.None;
    }

    public void ApplyLevel(float spawnChance, int spawnDamage, BallType spawnedBallType)
    {
        SpawnChance = Mathf.Clamp01(spawnChance);
        SpawnDamage = Mathf.Max(0, spawnDamage);
        SpawnedBallType = spawnedBallType;
    }
}
