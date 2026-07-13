using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ClusterBallGrowthDefinition", menuName = "Pinball/Ball/Growth Definitions/Cluster")]
public sealed class ClusterBallGrowthDefinitionSO : BallGrowthDefinitionSO
{
    [SerializeField] private ClusterBallGrowthLevelData[] levels;

    public override int LevelCount => GetLevelCount(levels);

    public override int GetLevel(int index) => GetLevelAt(levels, index);

    public override bool ApplyLevel(int level, BallRuntimeStat runtimeStat)
    {
        if (runtimeStat == null || !TryGetLevelData(levels, level, out ClusterBallGrowthLevelData levelData))
        {
            return false;
        }

        ApplyCommon(levelData, runtimeStat);

        ClusterEffectRuntimeStat cluster = runtimeStat.GetEffect<ClusterEffectRuntimeStat>();

        if (cluster != null)
        {
            cluster.ApplyGrowth(levelData.SpawnDamageAdd, levelData.SpawnDamageMultiplier);
        }

        return true;
    }
}

[Serializable]
public sealed class ClusterBallGrowthLevelData : BallGrowthLevelData
{
    [SerializeField] private int spawnDamageAdd;
    [SerializeField] private float spawnDamageMultiplier = 1f;

    public int SpawnDamageAdd => spawnDamageAdd;
    public float SpawnDamageMultiplier => spawnDamageMultiplier <= 0f ? 1f : spawnDamageMultiplier;
}
