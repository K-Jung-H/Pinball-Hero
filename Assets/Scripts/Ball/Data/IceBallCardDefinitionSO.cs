using System;
using UnityEngine;

[CreateAssetMenu(fileName = "IceBallCardDefinition", menuName = "Pinball/Ball/Card Definitions/Ice")]
public sealed class IceBallCardDefinitionSO : BallCardDefinitionSO
{
    [SerializeField] private IceBallCardLevelData[] levels;

    public override bool ApplyLevel(int level, BallRuntimeStat runtimeStat)
    {
        if (runtimeStat == null || !TryGetLevelData(level, out IceBallCardLevelData levelData))
        {
            return false;
        }

        runtimeStat.ApplyHitDamageModifier(levelData.HitDamageAdd, levelData.HitDamageMultiplier);
        runtimeStat.ApplyMoveSpeedMultiplier(levelData.MoveSpeedMultiplier);

        FreezeEffectRuntimeStat freeze = runtimeStat.GetEffect<FreezeEffectRuntimeStat>();

        if (freeze != null)
        {
            freeze.ApplyLevel(
                levelData.FreezeChance,
                levelData.FreezeDuration,
                levelData.SlowRatio,
                levelData.ExtraDamageRatio);
        }

        return true;
    }

    public override SkillDescriptionValue GetDescriptionValue(int level)
    {
        if (!TryGetLevelData(level, out IceBallCardLevelData levelData))
        {
            return SkillDescriptionValue.Zero;
        }

        return new SkillDescriptionValue
        {
            HitDamage = levelData.HitDamageAdd,
            ChancePercent = SkillDefinitionSO.ToPercent(levelData.FreezeChance),
            Duration = levelData.FreezeDuration,
            SlowPercent = SkillDefinitionSO.ToPercent(levelData.SlowRatio),
            ExtraDamagePercent = SkillDefinitionSO.ToPercent(levelData.ExtraDamageRatio)
        };
    }

    private bool TryGetLevelData(int level, out IceBallCardLevelData levelData)
    {
        levelData = null;

        if (levels == null)
        {
            return false;
        }

        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null && levels[i].Level == level)
            {
                levelData = levels[i];
                return true;
            }
        }

        return false;
    }
}

[Serializable]
public sealed class IceBallCardLevelData
{
    [SerializeField] private int level = 1;
    [SerializeField] private int hitDamageAdd;
    [SerializeField] private float hitDamageMultiplier = 1f;
    [SerializeField] private float moveSpeedMultiplier = 1f;
    [SerializeField] private float freezeChance;
    [SerializeField] private float freezeDuration;
    [SerializeField] private float slowRatio;
    [SerializeField] private float extraDamageRatio;

    public int Level => level;
    public int HitDamageAdd => hitDamageAdd;
    public float HitDamageMultiplier => hitDamageMultiplier <= 0f ? 1f : hitDamageMultiplier;
    public float MoveSpeedMultiplier => moveSpeedMultiplier <= 0f ? 1f : moveSpeedMultiplier;
    public float FreezeChance => freezeChance;
    public float FreezeDuration => freezeDuration;
    public float SlowRatio => slowRatio;
    public float ExtraDamageRatio => extraDamageRatio;
}
