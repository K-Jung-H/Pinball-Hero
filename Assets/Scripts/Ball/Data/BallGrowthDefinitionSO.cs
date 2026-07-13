using System;
using UnityEngine;

public abstract class BallGrowthDefinitionSO : ScriptableObject
{
    [SerializeField] private BallType ballType = BallType.Normal;

    public BallType BallType => ballType;

    public abstract bool ApplyLevel(int level, BallRuntimeStat runtimeStat);

    protected static bool TryGetLevelData<T>(T[] levels, int level, out T levelData)
        where T : BallGrowthLevelData
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

    protected static void ApplyCommon(BallGrowthLevelData levelData, BallRuntimeStat runtimeStat)
    {
        runtimeStat.ApplyHitDamageModifier(levelData.HitDamageAdd, levelData.HitDamageMultiplier);
        runtimeStat.ApplyMoveSpeedMultiplier(levelData.MoveSpeedMultiplier);
    }
}

[Serializable]
public class BallGrowthLevelData
{
    [SerializeField] private int level = 1;
    [SerializeField] private int hitDamageAdd;
    [SerializeField] private float hitDamageMultiplier = 1f;
    [SerializeField] private float moveSpeedMultiplier = 1f;

    public int Level => level;
    public int HitDamageAdd => hitDamageAdd;
    public float HitDamageMultiplier => hitDamageMultiplier <= 0f ? 1f : hitDamageMultiplier;
    public float MoveSpeedMultiplier => moveSpeedMultiplier <= 0f ? 1f : moveSpeedMultiplier;
}
