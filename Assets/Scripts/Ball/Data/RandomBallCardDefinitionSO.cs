using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomBallCardDefinition", menuName = "Pinball/Ball/Card Definitions/Random")]
public sealed class RandomBallCardDefinitionSO : BallCardDefinitionSO
{
    [SerializeField] private RandomBallCardLevelData[] levels;

    public override bool ApplyLevel(int level, BallRuntimeStat runtimeStat)
    {
        if (runtimeStat == null
            || !TryGetLevelData(level, out RandomBallCardLevelData levelData))
        {
            return false;
        }

        runtimeStat.ApplyHitDamageModifier(
            levelData.HitDamageAdd,
            levelData.HitDamageMultiplier);
        runtimeStat.ApplyMoveSpeedMultiplier(levelData.MoveSpeedMultiplier);

        RandomEffectRuntimeStat random = runtimeStat.GetEffect<RandomEffectRuntimeStat>();

        if (random != null)
        {
            random.SetCardLevel(
                levelData.DamageDecreaseRatio,
                levelData.DamageIncreaseRatio,
                levelData.SpeedDecreaseRatio,
                levelData.SpeedIncreaseRatio,
                levelData.AngleVariance);
        }

        return true;
    }

    public override SkillDescriptionValue GetDescriptionValue(int level)
    {
        if (!TryGetLevelData(level, out RandomBallCardLevelData levelData))
        {
            return SkillDescriptionValue.Zero;
        }

        return new SkillDescriptionValue
        {
            HitDamage = levelData.HitDamageAdd,
            DamageDecreasePercent = SkillDefinitionSO.ToPercent(levelData.DamageDecreaseRatio),
            DamageIncreasePercent = SkillDefinitionSO.ToPercent(levelData.DamageIncreaseRatio),
            SpeedDecreasePercent = SkillDefinitionSO.ToPercent(levelData.SpeedDecreaseRatio),
            SpeedIncreasePercent = SkillDefinitionSO.ToPercent(levelData.SpeedIncreaseRatio),
            AngleVariance = levelData.AngleVariance
        };
    }

    private bool TryGetLevelData(int level, out RandomBallCardLevelData levelData)
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
public sealed class RandomBallCardLevelData
{
    [SerializeField] private int level = 1;
    [SerializeField] private int hitDamageAdd;
    [SerializeField] private float hitDamageMultiplier = 1f;
    [SerializeField] private float moveSpeedMultiplier = 1f;
    [SerializeField, Range(0f, 0.95f)] private float damageDecreaseRatio;
    [SerializeField, Min(0f)] private float damageIncreaseRatio;
    [SerializeField, Range(0f, 0.95f)] private float speedDecreaseRatio;
    [SerializeField, Min(0f)] private float speedIncreaseRatio;
    [SerializeField, Range(0f, 89f)] private float angleVariance;

    public int Level => level;
    public int HitDamageAdd => hitDamageAdd;
    public float HitDamageMultiplier => hitDamageMultiplier <= 0f ? 1f : hitDamageMultiplier;
    public float MoveSpeedMultiplier => moveSpeedMultiplier <= 0f ? 1f : moveSpeedMultiplier;
    public float DamageDecreaseRatio => Mathf.Clamp(damageDecreaseRatio, 0f, 0.95f);
    public float DamageIncreaseRatio => Mathf.Max(0f, damageIncreaseRatio);
    public float SpeedDecreaseRatio => Mathf.Clamp(speedDecreaseRatio, 0f, 0.95f);
    public float SpeedIncreaseRatio => Mathf.Max(0f, speedIncreaseRatio);
    public float AngleVariance => Mathf.Clamp(angleVariance, 0f, 89f);
}
