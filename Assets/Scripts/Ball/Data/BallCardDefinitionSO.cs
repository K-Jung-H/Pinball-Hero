using UnityEngine;

public abstract class BallCardDefinitionSO : ScriptableObject
{
    [SerializeField] private BallType ballType = BallType.Normal;

    public BallType BallType => ballType;

    public abstract bool ApplyLevel(int level, BallRuntimeStat runtimeStat);
    public abstract SkillDescriptionValue GetDescriptionValue(int level);
}
