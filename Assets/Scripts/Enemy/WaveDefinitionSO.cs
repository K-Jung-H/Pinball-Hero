using UnityEngine;

[CreateAssetMenu(fileName = "WaveDefinitionSO", menuName = "Pinball/WaveDefinitionSO")]
public class WaveDefinitionSO : ScriptableObject
{
    [SerializeField] private int boardWidth;
    [SerializeField] private int waveHeight;
    [SerializeField] private WaveSpawnEntry[] spawnEntries;

    public int BoardWidth => boardWidth;
    public int WaveHeight => waveHeight;
    public WaveSpawnEntry[] SpawnEntries => spawnEntries;
}

[System.Serializable]
public struct WaveSpawnEntry
{
    [SerializeField] private EnemyDefinitionSO enemy;
    [SerializeField] private int column;
    [SerializeField] private int row;

    public EnemyDefinitionSO Enemy => enemy;
    public int Column => column;
    public int Row => row;
}
