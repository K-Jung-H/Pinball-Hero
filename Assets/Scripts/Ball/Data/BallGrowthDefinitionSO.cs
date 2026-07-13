using System;
using UnityEngine;

public abstract class BallGrowthDefinitionSO : ScriptableObject
{
    [SerializeField] private BallType ballType = BallType.Normal;

    public BallType BallType => ballType;
    public abstract int LevelCount { get; }
    public bool HasValidLevelRange
    {
        get
        {
            if (LevelCount <= 0)
            {
                return false;
            }

            int previousLevel = 0;

            for (int i = 0; i < LevelCount; i++)
            {
                int level = GetLevel(i);

                if (level <= previousLevel)
                {
                    return false;
                }

                previousLevel = level;
            }

            return true;
        }
    }

    public abstract int GetLevel(int index);
    public abstract bool ApplyLevel(int level, BallRuntimeStat runtimeStat);

    public bool TryGetLevelIndex(int level, out int index)
    {
        for (int i = 0; i < LevelCount; i++)
        {
            if (GetLevel(i) == level)
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }

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

    protected static int GetLevelCount<T>(T[] levels)
        where T : BallGrowthLevelData
    {
        return levels != null ? levels.Length : 0;
    }

    protected static int GetLevelAt<T>(T[] levels, int index)
        where T : BallGrowthLevelData
    {
        if (levels == null
            || index < 0
            || index >= levels.Length
            || levels[index] == null)
        {
            return 0;
        }

        return levels[index].Level;
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
