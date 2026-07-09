using UnityEngine;

public class StageBuilder : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CombatPipeline combatPipeline;

    private void Awake()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController is not assigned.");
            return;
        }

        BallShooter ballShooter = playerController.BallShooter;

        if (ballShooter == null)
        {
            Debug.LogError("BallShooter is not assigned in PlayerController.");
            return;
        }

        ballShooter.SetCombatPipeline(combatPipeline);
    }
}
