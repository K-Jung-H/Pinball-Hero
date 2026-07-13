using System;
using UnityEngine;

[CreateAssetMenu(fileName = "IceBallGrowthDefinition", menuName = "Pinball/Ball/Growth Definitions/Ice")]
public sealed class IceBallGrowthDefinitionSO : BallGrowthDefinitionSO
{
    [SerializeField] private IceBallGrowthLevelData[] levels;

    public override bool ApplyLevel(int level, BallRuntimeStat runtimeStat)
    {
        if (runtimeStat == null || !TryGetLevelData(levels, level, out IceBallGrowthLevelData levelData))
        {
            return false;
        }

        ApplyCommon(levelData, runtimeStat);

        FreezeEffectRuntimeStat freeze = runtimeStat.GetEffect<FreezeEffectRuntimeStat>();

        if (freeze != null)
        {
            freeze.ApplyGrowth(levelData.ExtraDamageRatioAdd, levelData.ExtraDamageRatioMultiplier);
        }

        return true;
    }
}

[Serializable]
public sealed class IceBallGrowthLevelData : BallGrowthLevelData
{
    [SerializeField] private float extraDamageRatioAdd;
    [SerializeField] private float extraDamageRatioMultiplier = 1f;

    public float ExtraDamageRatioAdd => extraDamageRatioAdd;
    public float ExtraDamageRatioMultiplier => extraDamageRatioMultiplier <= 0f
        ? 1f
        : extraDamageRatioMultiplier;
}
