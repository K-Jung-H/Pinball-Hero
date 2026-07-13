using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "DeathExplosionPassiveDefinition",
    menuName = "Pinball/Skill/Passive/Death Explosion")]
public sealed class DeathExplosionPassiveDefinitionSO : PassiveSkillDefinitionSO
{
    [SerializeField] private DamageArea areaPrefab;
    [SerializeField] private Vector2Int areaSizeInCells = new Vector2Int(3, 3);
    [SerializeField] private DeathExplosionPassiveLevelData[] levels;

    public DamageArea AreaPrefab => areaPrefab;
    public Vector2 AreaSizeInCells => new Vector2(
        Mathf.Max(1, areaSizeInCells.x),
        Mathf.Max(1, areaSizeInCells.y));

    public bool TryGetLevelData(int level, out DeathExplosionPassiveLevelData levelData)
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

        if (TryGetLevelData(level, out DeathExplosionPassiveLevelData levelData))
        {
            value.EffectDamage = levelData.Damage;
        }

        return value;
    }
}

[Serializable]
public sealed class DeathExplosionPassiveLevelData
{
    [SerializeField] private int level = 1;
    [SerializeField] private int damage;

    public int Level => level;
    public int Damage => Mathf.Max(0, damage);
}
