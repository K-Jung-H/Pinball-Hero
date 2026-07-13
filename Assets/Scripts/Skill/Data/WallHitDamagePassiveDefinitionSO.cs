using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "WallHitDamagePassiveDefinition",
    menuName = "Pinball/Skill/Passive/Wall Hit Damage")]
public sealed class WallHitDamagePassiveDefinitionSO : PassiveSkillDefinitionSO
{
    [SerializeField] private WallHitDamagePassiveLevelData[] levels;

    public override float GetDamageBonusRatio(int level, DamageRequest request)
    {
        if (request.SourceType != DamageSourceType.DirectHit
            || request.WallHitCount <= 0
            || !TryGetLevelData(level, out WallHitDamagePassiveLevelData levelData))
        {
            return 0f;
        }

        return levelData.DamageBonusRatioPerWall * request.WallHitCount;
    }

    public bool TryGetLevelData(int level, out WallHitDamagePassiveLevelData levelData)
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

        if (TryGetLevelData(level, out WallHitDamagePassiveLevelData levelData))
        {
            value.DamageMultiplierPercent = ToPercent(levelData.DamageBonusRatioPerWall);
        }

        return value;
    }
}

[Serializable]
public sealed class WallHitDamagePassiveLevelData
{
    [SerializeField] private int level = 1;
    [SerializeField] private float damageBonusRatioPerWall;

    public int Level => level;
    public float DamageBonusRatioPerWall => Mathf.Max(0f, damageBonusRatioPerWall);
}
