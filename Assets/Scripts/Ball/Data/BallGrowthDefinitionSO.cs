using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BallGrowthDefinition", menuName = "Pinball/Ball/Growth Definition")]
public class BallGrowthDefinitionSO : ScriptableObject
{
    [SerializeField] private BallType ballType = BallType.Normal;
    [SerializeField] private BallGrowthLevelData[] levels;

    public BallType BallType => ballType;

    public bool TryGetLevelData(int level, out BallGrowthLevelData levelData)
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
public sealed class BallGrowthLevelData
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
