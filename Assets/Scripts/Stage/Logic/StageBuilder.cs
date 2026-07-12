using UnityEngine;

public class StageBuilder : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CombatPipeline combatPipeline;
    [SerializeField] private DamageTextSystem damageTextSystem;
    [SerializeField] private SkillSelectionController skillSelectionController;
    [SerializeField] private SkillCatalogSO skillCatalog;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private StageDefinitionSO stageDefinition;
    [SerializeField] private StageBallProgressSetSO stageBallProgressSet;

    private void Awake()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController is not assigned.");
            return;
        }

        if (combatPipeline == null)
        {
            Debug.LogError("CombatPipeline is not assigned.");
            return;
        }

        BallShooter ballShooter = playerController.BallShooter;

        if (ballShooter == null)
        {
            Debug.LogError("BallShooter is not assigned in PlayerController.");
            return;
        }

        if (skillSelectionController == null)
        {
            Debug.LogError("SkillSelectionController is not assigned.");
            return;
        }

        ballShooter.SetCombatPipeline(combatPipeline);
        StageBallProgress[] stageBallProgresses = stageBallProgressSet != null
            ? stageBallProgressSet.Progresses
            : null;

        skillSelectionController.Initialize(skillCatalog, ballShooter);
        RunSkillInventory runSkillInventory = skillSelectionController.Inventory;

        ballShooter.SetRuntimeData(stageBallProgresses, runSkillInventory);
        combatPipeline.SetRunSkillInventory(runSkillInventory);

        if (damageTextSystem != null)
        {
            damageTextSystem.SetCombatPipeline(combatPipeline);
        }

        if (waveSpawner == null)
        {
            Debug.LogError("WaveSpawner is not assigned.");
            return;
        }

        waveSpawner.SetAttackTarget(playerController.transform);
        waveSpawner.StartStage(stageDefinition);
    }
}
