using UnityEngine;

[CreateAssetMenu(fileName = "BasicBallGrowthDefinition", menuName = "Pinball/Ball/Growth Definitions/Basic")]
public sealed class BasicBallGrowthDefinitionSO : BallGrowthDefinitionSO
{
    [SerializeField] private BallGrowthLevelData[] levels;

    public override int LevelCount => GetLevelCount(levels);

    public override int GetLevel(int index) => GetLevelAt(levels, index);

    public override bool ApplyLevel(int level, BallRuntimeStat runtimeStat)
    {
        if (runtimeStat == null || !TryGetLevelData(levels, level, out BallGrowthLevelData levelData))
        {
            return false;
        }

        ApplyCommon(levelData, runtimeStat);
        return true;
    }
}
