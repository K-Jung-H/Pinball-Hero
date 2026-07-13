using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "DirectionalCritPassiveDefinition",
    menuName = "Pinball/Skill/Passive/Directional Critical")]
public sealed class DirectionalCritPassiveDefinitionSO : PassiveSkillDefinitionSO
{
    [SerializeField] private EnemyHitSide targetSide = EnemyHitSide.Front;
    [SerializeField] private DirectionalCritPassiveLevelData[] levels;

    public override float GetCritChanceBonus(int level, DamageRequest request)
    {
        if (!request.CanCrit
            || request.SourceType != DamageSourceType.DirectHit
            || request.HitSide != targetSide
            || !TryGetLevelData(level, out DirectionalCritPassiveLevelData levelData))
        {
            return 0f;
        }

        return levelData.CritChanceBonus;
    }

    public bool TryGetLevelData(int level, out DirectionalCritPassiveLevelData levelData)
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

        if (TryGetLevelData(level, out DirectionalCritPassiveLevelData levelData))
        {
            value.ChancePercent = ToPercent(levelData.CritChanceBonus);
        }

        return value;
    }
}

[Serializable]
public sealed class DirectionalCritPassiveLevelData
{
    [SerializeField] private int level = 1;
    [SerializeField] private float critChanceBonus;

    public int Level => level;
    public float CritChanceBonus => Mathf.Clamp01(critChanceBonus);
}
