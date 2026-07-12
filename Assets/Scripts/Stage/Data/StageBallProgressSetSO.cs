using UnityEngine;

[CreateAssetMenu(fileName = "StageBallProgressSet", menuName = "Pinball/Stage/Ball Progress Set")]
public class StageBallProgressSetSO : ScriptableObject
{
    [SerializeField] private StageBallProgress[] progresses;

    public StageBallProgress[] Progresses => progresses;
}
