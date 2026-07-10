using UnityEngine;

public class StageBuilder : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CombatPipeline combatPipeline;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private StageDefinitionSO stageDefinition;

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

        ballShooter.SetCombatPipeline(combatPipeline);

        if (waveSpawner == null)
        {
            Debug.LogError("WaveSpawner is not assigned.");
            return;
        }

        waveSpawner.StartStage(stageDefinition);
    }
}
