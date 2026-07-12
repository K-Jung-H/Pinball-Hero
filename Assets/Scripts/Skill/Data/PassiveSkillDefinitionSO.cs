using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PassiveSkillDefinition", menuName = "Pinball/Skill/Passive Definition")]
public sealed class PassiveSkillDefinitionSO : SkillDefinitionSO
{
    [SerializeField] private PassiveSkillLevelData[] levels;

    public override SkillCategory Category => SkillCategory.Passive;

    public bool TryGetLevelData(int level, out PassiveSkillLevelData levelData)
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

        if (level <= 0 || !TryGetLevelData(level, out PassiveSkillLevelData levelData))
        {
            return value;
        }

        value.PassiveDamageAdd = levelData.DamageAdd;
        value.DamageMultiplierPercent = ToPercent(levelData.DamageMultiplier - 1f);
        return value;
    }
}

[Serializable]
public sealed class PassiveSkillLevelData
{
    [SerializeField] private int level = 1;
    [SerializeField] private bool filterBallType;
    [SerializeField] private BallType targetBallType = BallType.None;
    [SerializeField] private bool filterDamageType;
    [SerializeField] private DamageType targetDamageType = DamageType.Normal;
    [SerializeField] private bool filterSourceType;
    [SerializeField] private DamageSourceType targetSourceType = DamageSourceType.DirectHit;
    [SerializeField] private int damageAdd;
    [SerializeField] private float damageMultiplier = 1f;

    public int Level => level;
    public int DamageAdd => damageAdd;
    public float DamageMultiplier => damageMultiplier <= 0f ? 1f : damageMultiplier;

    public bool Matches(DamageRequest request)
    {
        if (filterBallType && request.BallType != targetBallType)
        {
            return false;
        }

        if (filterDamageType && request.DamageType != targetDamageType)
        {
            return false;
        }

        if (filterSourceType && request.SourceType != targetSourceType)
        {
            return false;
        }

        return true;
    }
}
