using UnityEngine;

public sealed class StageResultController : MonoBehaviour
{
    [SerializeField] private StageResultUI resultUI;

    private PlayerController playerController;
    private WaveSpawner waveSpawner;
    private bool isFinished;

    public bool Initialize(PlayerController player, WaveSpawner spawner)
    {
        Unbind();

        playerController = player;
        waveSpawner = spawner;
        isFinished = false;

        if (playerController == null || waveSpawner == null || resultUI == null)
        {
            return false;
        }

        resultUI.Hide();
        playerController.PlayerDied += OnPlayerDied;
        waveSpawner.StageCompleted += OnStageCompleted;
        return true;
    }

    public void Unbind()
    {
        if (playerController != null)
        {
            playerController.PlayerDied -= OnPlayerDied;
        }

        if (waveSpawner != null)
        {
            waveSpawner.StageCompleted -= OnStageCompleted;
        }

        playerController = null;
        waveSpawner = null;
    }

    private void OnPlayerDied()
    {
        Finish(false);
    }

    private void OnStageCompleted()
    {
        Finish(playerController != null && !playerController.IsDead);
    }

    private void Finish(bool isSuccess)
    {
        if (isFinished)
        {
            return;
        }

        isFinished = true;
        Time.timeScale = 0f;

        if (isSuccess)
        {
            resultUI.ShowSuccess();
        }
        else
        {
            resultUI.ShowFail();
        }
    }

    private void OnDestroy()
    {
        Unbind();
    }
}
