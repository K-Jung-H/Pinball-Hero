using UnityEngine;

public sealed class DeathEffectSystem
{
    private readonly RunSkillInventory runSkillInventory;
    private readonly CombatPipeline combatPipeline;
    private readonly AreaEffectSystem areaEffectSystem;

    public DeathEffectSystem(
        RunSkillInventory inventory,
        CombatPipeline pipeline,
        AreaEffectSystem effectSystem)
    {
        runSkillInventory = inventory;
        combatPipeline = pipeline;
        areaEffectSystem = effectSystem;
    }

    public void Process(Enemy_Base enemy)
    {
        if (enemy == null
            || runSkillInventory == null
            || combatPipeline == null
            || areaEffectSystem == null
            || !runSkillInventory.TryGetPassive(
                out DeathExplosionPassiveDefinitionSO definition,
                out int level)
            || !definition.TryGetLevelData(
                level,
                out DeathExplosionPassiveLevelData levelData)
            || definition.AreaPrefab == null
            || levelData.Damage <= 0)
        {
            return;
        }

        Vector2 areaSize = Vector2.Scale(
            areaEffectSystem.CellSize,
            definition.AreaSizeInCells);

        areaEffectSystem.Play(
            definition.AreaPrefab,
            combatPipeline,
            enemy.Center,
            areaSize,
            levelData.Damage,
            BallType.None,
            DamageType.Explosion);
    }
}
