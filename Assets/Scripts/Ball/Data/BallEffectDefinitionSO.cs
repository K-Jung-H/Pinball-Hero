using UnityEngine;

public abstract class BallEffectDefinitionSO : ScriptableObject
{
    public abstract BallEffectType EffectType { get; }
    public abstract BallEffectRuntimeStat CreateRuntimeStat();
}
