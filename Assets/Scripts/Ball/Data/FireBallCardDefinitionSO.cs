using System;
using UnityEngine;

[CreateAssetMenu(fileName = "FireBallCardDefinition", menuName = "Pinball/Ball/Card Definitions/Fire")]
public sealed class FireBallCardDefinitionSO : BallCardDefinitionSO
{
    [SerializeField] private FireBallCardLevelData[] levels;

    public override bool ApplyLevel(int level, BallRuntimeStat runtimeStat)
    {
        if (runtimeStat == null || !TryGetLevelData(level, out FireBallCardLevelData levelData))
        {
            return false;
        }

        runtimeStat.ApplyHitDamageModifier(levelData.HitDamageAdd, levelData.HitDamageMultiplier);
        runtimeStat.ApplyMoveSpeedMultiplier(levelData.MoveSpeedMultiplier);

        BurnEffectRuntimeStat burn = runtimeStat.GetEffect<BurnEffectRuntimeStat>();

        if (burn != null)
        {
            burn.SetCardLevel(levelData.BurnDotDamage, levelData.BurnDuration, levelData.BurnMaxStack);
        }

        return true;
    }

    public override SkillDescriptionValue GetDescriptionValue(int level)
    {
        if (!TryGetLevelData(level, out FireBallCardLevelData levelData))
        {
            return SkillDescriptionValue.Zero;
        }

        return new SkillDescriptionValue
        {
            HitDamage = levelData.HitDamageAdd,
            EffectDamage = levelData.BurnDotDamage,
            Duration = levelData.BurnDuration,
            MaxStack = levelData.BurnMaxStack
        };
    }

    private bool TryGetLevelData(int level, out FireBallCardLevelData levelData)
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
public sealed class FireBallCardLevelData
{
    [SerializeField] private int level = 1;
    [SerializeField] private int hitDamageAdd;
    [SerializeField] private float hitDamageMultiplier = 1f;
    [SerializeField] private float moveSpeedMultiplier = 1f;
    [SerializeField] private int burnDotDamage;
    [SerializeField] private float burnDuration;
    [SerializeField] private int burnMaxStack = 1;

    public int Level => level;
    public int HitDamageAdd => hitDamageAdd;
    public float HitDamageMultiplier => hitDamageMultiplier <= 0f ? 1f : hitDamageMultiplier;
    public float MoveSpeedMultiplier => moveSpeedMultiplier <= 0f ? 1f : moveSpeedMultiplier;
    public int BurnDotDamage => burnDotDamage;
    public float BurnDuration => burnDuration;
    public int BurnMaxStack => burnMaxStack;
}
