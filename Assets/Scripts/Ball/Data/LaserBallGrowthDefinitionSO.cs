using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LaserBallGrowthDefinition", menuName = "Pinball/Ball/Growth Definitions/Laser")]
public sealed class LaserBallGrowthDefinitionSO : BallGrowthDefinitionSO
{
    [SerializeField] private LaserBallGrowthLevelData[] levels;

    public override int LevelCount => GetLevelCount(levels);

    public override int GetLevel(int index) => GetLevelAt(levels, index);

    public override bool ApplyLevel(int level, BallRuntimeStat runtimeStat)
    {
        if (runtimeStat == null || !TryGetLevelData(levels, level, out LaserBallGrowthLevelData levelData))
        {
            return false;
        }

        ApplyCommon(levelData, runtimeStat);

        LaserEffectRuntimeStat laser = runtimeStat.GetEffect<LaserEffectRuntimeStat>();

        if (laser != null)
        {
            laser.ApplyGrowth(levelData.RowDamageAdd, levelData.RowDamageMultiplier);
        }

        return true;
    }
}

[Serializable]
public sealed class LaserBallGrowthLevelData : BallGrowthLevelData
{
    [SerializeField] private int rowDamageAdd;
    [SerializeField] private float rowDamageMultiplier = 1f;

    public int RowDamageAdd => rowDamageAdd;
    public float RowDamageMultiplier => rowDamageMultiplier <= 0f ? 1f : rowDamageMultiplier;
}
