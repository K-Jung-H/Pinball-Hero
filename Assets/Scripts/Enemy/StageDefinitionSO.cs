using UnityEngine;

[CreateAssetMenu(fileName = "StageDefinitionSO", menuName = "Pinball/StageDefinitionSO")]
public class StageDefinitionSO : ScriptableObject
{
    [SerializeField] private int initialSpawnRows = 3;
    [SerializeField] private WaveDefinitionSO[] waves;

    public int InitialSpawnRows => initialSpawnRows;
    public WaveDefinitionSO[] Waves => waves;
}
