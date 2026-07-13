using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "BallDamagePassiveDefinition",
    menuName = "Pinball/Skill/Passive/Ball Damage")]
public sealed class BallDamagePassiveDefinitionSO : PassiveSkillDefinitionSO
{
    [SerializeField] private BallType targetBallType = BallType.Normal;
    [SerializeField] private DamageSourceType targetSourceType = DamageSourceType.DirectHit;
    [SerializeField] private BallDamagePassiveLevelData[] levels;

    public override float GetDamageBonusRatio(int level, DamageRequest request)
    {
        if (request.BallType != targetBallType
            || request.SourceType != targetSourceType
            || !TryGetLevelData(level, out BallDamagePassiveLevelData levelData))
        {
            return 0f;
        }

        return levelData.DamageBonusRatio;
    }

    public bool TryGetLevelData(int level, out BallDamagePassiveLevelData levelData)
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

    protected override SkillDescriptionValue GetDescriptionValue(int level)
    {
        SkillDescriptionValue value = SkillDescriptionValue.Zero;

        if (TryGetLevelData(level, out BallDamagePassiveLevelData levelData))
        {
            value.DamageMultiplierPercent = ToPercent(levelData.DamageBonusRatio);
        }

        return value;
    }
}

[Serializable]
public sealed class BallDamagePassiveLevelData
{
    [SerializeField] private int level = 1;
    [SerializeField] private float damageBonusRatio;

    public int Level => level;
    public float DamageBonusRatio => Mathf.Max(0f, damageBonusRatio);
}
