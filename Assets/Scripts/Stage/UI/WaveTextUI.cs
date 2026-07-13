using TMPro;
using UnityEngine;

public sealed class WaveTextUI : MonoBehaviour
{
    [SerializeField] private TMP_Text waveText;

    private WaveSpawner waveSpawner;

    public void Bind(WaveSpawner spawner)
    {
        if (waveSpawner != null)
        {
            waveSpawner.WaveStarted -= OnWaveStarted;
        }

        waveSpawner = spawner;

        if (waveSpawner != null)
        {
            waveSpawner.WaveStarted += OnWaveStarted;
        }
    }

    private void OnWaveStarted(int waveNumber)
    {
        if (waveText != null)
        {
            waveText.SetText("Wave {0}", waveNumber);
        }
    }

    private void OnDestroy()
    {
        Bind(null);
    }
}
