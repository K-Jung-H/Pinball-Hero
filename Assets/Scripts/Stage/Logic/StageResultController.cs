using UnityEngine;
using UnityEngine.InputSystem;

public sealed class StageResultController : MonoBehaviour
{
    [SerializeField] private StageResultUI resultUI;

    private PlayerController playerController;
    private WaveSpawner waveSpawner;
    private bool isFinished;
    private bool isPaused;
    private float previousTimeScale = 1f;

    private void Update()
    {
        if (Keyboard.current != null
            && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Pause();
        }
    }

    public bool Initialize(PlayerController player, WaveSpawner spawner)
    {
        Unbind();

        playerController = player;
        waveSpawner = spawner;
        isFinished = false;
        isPaused = false;
        previousTimeScale = 1f;

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

    public void ResetRun()
    {
        isFinished = false;
        isPaused = false;
        previousTimeScale = 1f;

        if (resultUI != null)
        {
            resultUI.Hide();
        }
    }

    public void Pause()
    {
        if (isFinished || isPaused || Time.timeScale <= 0f)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isPaused = true;
        resultUI.ShowPause();
    }

    public void Continue()
    {
        if (isFinished || !isPaused)
        {
            return;
        }

        isPaused = false;
        resultUI.Hide();
        Time.timeScale = previousTimeScale > 0f
            ? previousTimeScale
            : 1f;
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
        isPaused = false;
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
