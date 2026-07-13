using UnityEngine;

public sealed class StageRunController : MonoBehaviour
{
    private PlayerController playerController;
    private BallShooter ballShooter;
    private CombatPipeline combatPipeline;
    private DamageTextSystem damageTextSystem;
    private AreaEffectSystem areaEffectSystem;
    private SkillSelectionController skillSelectionController;
    private StageExperienceSystem stageExperienceSystem;
    private ExperienceBarUI experienceBarUI;
    private StageResultController stageResultController;
    private WaveSpawner waveSpawner;
    private StageDefinitionSO stageDefinition;
    private StageBallProgressSetSO fallbackProgressSet;
    private bool isInitialized;

    public bool Initialize(
        PlayerController player,
        BallShooter shooter,
        CombatPipeline pipeline,
        DamageTextSystem damageText,
        AreaEffectSystem areaEffect,
        SkillSelectionController skillSelection,
        StageExperienceSystem experienceSystem,
        ExperienceBarUI experienceBar,
        StageResultController resultController,
        WaveSpawner spawner,
        StageDefinitionSO definition,
        StageBallProgressSetSO progressSet)
    {
        playerController = player;
        ballShooter = shooter;
        combatPipeline = pipeline;
        damageTextSystem = damageText;
        areaEffectSystem = areaEffect;
        skillSelectionController = skillSelection;
        stageExperienceSystem = experienceSystem;
        experienceBarUI = experienceBar;
        stageResultController = resultController;
        waveSpawner = spawner;
        stageDefinition = definition;
        fallbackProgressSet = progressSet;

        isInitialized = playerController != null
            && ballShooter != null
            && combatPipeline != null
            && skillSelectionController != null
            && stageExperienceSystem != null
            && experienceBarUI != null
            && stageResultController != null
            && waveSpawner != null
            && stageDefinition != null;

        return isInitialized;
    }

    public void StartRun()
    {
        if (!isInitialized)
        {
            return;
        }

        Time.timeScale = 1f;

        waveSpawner.ResetRun();
        skillSelectionController.ResetRun();
        damageTextSystem?.ResetRun();
        areaEffectSystem?.ResetRun();
        playerController.ResetRun();
        stageResultController.ResetRun();

        RunSkillInventory runSkillInventory = skillSelectionController.Inventory;
        StageBallProgress[] stageBallProgresses = PlayerProgressSession.HasProgress
            ? PlayerProgressSession.CreateStageBallProgresses()
            : fallbackProgressSet != null
                ? fallbackProgressSet.Progresses
                : null;

        ballShooter.ResetRun(stageBallProgresses, runSkillInventory);
        combatPipeline.SetRunSkillInventory(runSkillInventory);

        experienceBarUI.Bind(stageExperienceSystem);

        if (!stageExperienceSystem.Initialize(stageDefinition, skillSelectionController))
        {
            Debug.LogError("Stage experience data is not configured.");
            experienceBarUI.Bind(null);
            return;
        }

        waveSpawner.SetAttackTarget(playerController.transform);
        waveSpawner.StartStage(stageDefinition);
    }

    public void Retry()
    {
        StartRun();
    }
}
