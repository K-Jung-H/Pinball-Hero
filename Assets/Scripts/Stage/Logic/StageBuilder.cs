using UnityEngine;

public class StageBuilder : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CombatPipeline combatPipeline;
    [SerializeField] private DamageTextSystem damageTextSystem;
    [SerializeField] private AreaEffectSystem areaEffectSystem;
    [SerializeField] private SkillSelectionController skillSelectionController;
    [SerializeField] private SkillCatalogSO skillCatalog;
    [SerializeField] private StageExperienceSystem stageExperienceSystem;
    [SerializeField] private ExperienceBarUI experienceBarUI;
    [SerializeField] private WaveTextUI waveTextUI;
    [SerializeField] private StageResultController stageResultController;
    [SerializeField] private StageRunController stageRunController;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private StageDefinitionSO stageDefinition;
    [SerializeField] private StageBallProgressSetSO stageBallProgressSet;

    private DeathEffectSystem deathEffectSystem;

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
        combatPipeline.SetBallShooter(ballShooter);

        skillSelectionController.Initialize(skillCatalog, ballShooter);
        RunSkillInventory runSkillInventory = skillSelectionController.Inventory;

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

        if (areaEffectSystem == null)
        {
            Debug.LogError("AreaEffectSystem is not assigned.");
            return;
        }

        areaEffectSystem.Initialize(waveSpawner.CellSize, waveSpawner.BoardBounds);
        combatPipeline.SetAreaEffectSystem(areaEffectSystem);

        if (stageExperienceSystem == null)
        {
            Debug.LogError("StageExperienceSystem is not assigned.");
            return;
        }

        if (experienceBarUI == null)
        {
            Debug.LogError("ExperienceBarUI is not assigned.");
            return;
        }

        if (waveTextUI == null)
        {
            Debug.LogError("WaveTextUI is not assigned.");
            return;
        }

        if (stageResultController == null)
        {
            Debug.LogError("StageResultController is not assigned.");
            return;
        }

        if (stageRunController == null)
        {
            Debug.LogError("StageRunController is not assigned.");
            return;
        }

        waveTextUI.Bind(waveSpawner);

        if (!stageResultController.Initialize(playerController, waveSpawner))
        {
            Debug.LogError("StageResultController is not configured.");
            return;
        }

        waveSpawner.ExperienceGained += stageExperienceSystem.AddExperience;
        waveSpawner.PlayerDamageRequested += playerController.TakeDamage;

        deathEffectSystem = new DeathEffectSystem(
            runSkillInventory,
            combatPipeline,
            areaEffectSystem);
        waveSpawner.EnemyDefeated += deathEffectSystem.Process;

        if (!stageRunController.Initialize(
            playerController,
            ballShooter,
            combatPipeline,
            damageTextSystem,
            areaEffectSystem,
            skillSelectionController,
            stageExperienceSystem,
            experienceBarUI,
            stageResultController,
            waveSpawner,
            stageDefinition,
            stageBallProgressSet))
        {
            Debug.LogError("StageRunController is not configured.");
            return;
        }

        stageRunController.StartRun();
    }

    private void OnDestroy()
    {
        if (waveSpawner != null && deathEffectSystem != null)
        {
            waveSpawner.EnemyDefeated -= deathEffectSystem.Process;
        }

        if (waveSpawner != null && stageExperienceSystem != null)
        {
            waveSpawner.ExperienceGained -= stageExperienceSystem.AddExperience;
        }

        if (waveSpawner != null && playerController != null)
        {
            waveSpawner.PlayerDamageRequested -= playerController.TakeDamage;
        }

        if (experienceBarUI != null)
        {
            experienceBarUI.Bind(null);
        }

        if (waveTextUI != null)
        {
            waveTextUI.Bind(null);
        }

        if (stageResultController != null)
        {
            stageResultController.Unbind();
        }
    }
}
