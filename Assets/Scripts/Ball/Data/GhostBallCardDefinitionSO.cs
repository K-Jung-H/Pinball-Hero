using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GhostBallCardDefinition", menuName = "Pinball/Ball/Card Definitions/Ghost")]
public sealed class GhostBallCardDefinitionSO : BallCardDefinitionSO
{
    [SerializeField] private GhostBallCardLevelData[] levels;

    public override bool ApplyLevel(int level, BallRuntimeStat runtimeStat)
    {
        if (runtimeStat == null || !TryGetLevelData(level, out GhostBallCardLevelData levelData))
        {
            return false;
        }

        runtimeStat.ApplyHitDamageModifier(levelData.HitDamageAdd, levelData.HitDamageMultiplier);
        runtimeStat.ApplyMoveSpeedMultiplier(levelData.MoveSpeedMultiplier);
        return true;
    }

    public override SkillDescriptionValue GetDescriptionValue(int level)
    {
        if (!TryGetLevelData(level, out GhostBallCardLevelData levelData))
        {
            return SkillDescriptionValue.Zero;
        }

        return new SkillDescriptionValue
        {
            HitDamage = levelData.HitDamageAdd
        };
    }

    private bool TryGetLevelData(int level, out GhostBallCardLevelData levelData)
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
public sealed class GhostBallCardLevelData
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
