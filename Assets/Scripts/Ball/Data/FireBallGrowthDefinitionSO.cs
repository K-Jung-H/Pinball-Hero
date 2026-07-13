using System;
using UnityEngine;

[CreateAssetMenu(fileName = "FireBallGrowthDefinition", menuName = "Pinball/Ball/Growth Definitions/Fire")]
public sealed class FireBallGrowthDefinitionSO : BallGrowthDefinitionSO
{
    [SerializeField] private FireBallGrowthLevelData[] levels;

    public override bool ApplyLevel(int level, BallRuntimeStat runtimeStat)
    {
        if (runtimeStat == null || !TryGetLevelData(levels, level, out FireBallGrowthLevelData levelData))
        {
            return false;
        }

        ApplyCommon(levelData, runtimeStat);

        BurnEffectRuntimeStat burn = runtimeStat.GetEffect<BurnEffectRuntimeStat>();

        if (burn != null)
        {
            burn.ApplyGrowth(levelData.DotDamageAdd, levelData.DotDamageMultiplier);
        }

        return true;
    }
}

[Serializable]
public sealed class FireBallGrowthLevelData : BallGrowthLevelData
{
    [SerializeField] private int dotDamageAdd;
    [SerializeField] private float dotDamageMultiplier = 1f;

    public int DotDamageAdd => dotDamageAdd;
    public float DotDamageMultiplier => dotDamageMultiplier <= 0f ? 1f : dotDamageMultiplier;
}
