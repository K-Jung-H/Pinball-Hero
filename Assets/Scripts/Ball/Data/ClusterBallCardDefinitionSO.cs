using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ClusterBallCardDefinition", menuName = "Pinball/Ball/Card Definitions/Cluster")]
public sealed class ClusterBallCardDefinitionSO : BallCardDefinitionSO
{
    [SerializeField] private ClusterBallCardLevelData[] levels;

    public override bool ApplyLevel(int level, BallRuntimeStat runtimeStat)
    {
        if (runtimeStat == null || !TryGetLevelData(level, out ClusterBallCardLevelData levelData))
        {
            return false;
        }

        runtimeStat.ApplyHitDamageModifier(levelData.HitDamageAdd, levelData.HitDamageMultiplier);
        runtimeStat.ApplyMoveSpeedMultiplier(levelData.MoveSpeedMultiplier);

        ClusterEffectRuntimeStat cluster = runtimeStat.GetEffect<ClusterEffectRuntimeStat>();

        if (cluster != null)
        {
            cluster.ApplyLevel(levelData.SpawnChance, levelData.SpawnDamage, levelData.SpawnedBallType);
        }

        return true;
    }

    public override SkillDescriptionValue GetDescriptionValue(int level)
    {
        if (!TryGetLevelData(level, out ClusterBallCardLevelData levelData))
        {
            return SkillDescriptionValue.Zero;
        }

        return new SkillDescriptionValue
        {
            HitDamage = levelData.HitDamageAdd,
            ChancePercent = SkillDefinitionSO.ToPercent(levelData.SpawnChance),
            EffectDamage = levelData.SpawnDamage
        };
    }

    private bool TryGetLevelData(int level, out ClusterBallCardLevelData levelData)
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
public sealed class ClusterBallCardLevelData
{
    [SerializeField] private int level = 1;
    [SerializeField] private int hitDamageAdd;
    [SerializeField] private float hitDamageMultiplier = 1f;
    [SerializeField] private float moveSpeedMultiplier = 1f;
    [SerializeField] private float spawnChance;
    [SerializeField] private int spawnDamage;
    [SerializeField] private BallType spawnedBallType = BallType.ClusterFragment;

    public int Level => level;
    public int HitDamageAdd => hitDamageAdd;
    public float HitDamageMultiplier => hitDamageMultiplier <= 0f ? 1f : hitDamageMultiplier;
    public float MoveSpeedMultiplier => moveSpeedMultiplier <= 0f ? 1f : moveSpeedMultiplier;
    public float SpawnChance => spawnChance;
    public int SpawnDamage => spawnDamage;
    public BallType SpawnedBallType => spawnedBallType;
}
