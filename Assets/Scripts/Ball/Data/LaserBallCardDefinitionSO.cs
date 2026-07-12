using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LaserBallCardDefinition", menuName = "Pinball/Ball/Card Definitions/Laser")]
public sealed class LaserBallCardDefinitionSO : BallCardDefinitionSO
{
    [SerializeField] private LaserBallCardLevelData[] levels;

    public override bool ApplyLevel(int level, BallRuntimeStat runtimeStat)
    {
        if (runtimeStat == null || !TryGetLevelData(level, out LaserBallCardLevelData levelData))
        {
            return false;
        }

        runtimeStat.ApplyHitDamageModifier(levelData.HitDamageAdd, levelData.HitDamageMultiplier);
        runtimeStat.ApplyMoveSpeedMultiplier(levelData.MoveSpeedMultiplier);

        LaserEffectRuntimeStat laser = runtimeStat.GetEffect<LaserEffectRuntimeStat>();

        if (laser != null)
        {
            laser.ApplyLevel(levelData.RowDamage);
        }

        return true;
    }

    public override SkillDescriptionValue GetDescriptionValue(int level)
    {
        if (!TryGetLevelData(level, out LaserBallCardLevelData levelData))
        {
            return SkillDescriptionValue.Zero;
        }

        return new SkillDescriptionValue
        {
            HitDamage = levelData.HitDamageAdd,
            EffectDamage = levelData.RowDamage
        };
    }

    private bool TryGetLevelData(int level, out LaserBallCardLevelData levelData)
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
public sealed class LaserBallCardLevelData
{
    [SerializeField] private int level = 1;
    [SerializeField] private int hitDamageAdd;
    [SerializeField] private float hitDamageMultiplier = 1f;
    [SerializeField] private float moveSpeedMultiplier = 1f;
    [SerializeField] private int rowDamage;

    public int Level => level;
    public int HitDamageAdd => hitDamageAdd;
    public float HitDamageMultiplier => hitDamageMultiplier <= 0f ? 1f : hitDamageMultiplier;
    public float MoveSpeedMultiplier => moveSpeedMultiplier <= 0f ? 1f : moveSpeedMultiplier;
    public int RowDamage => rowDamage;
}
