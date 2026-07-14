using UnityEngine;

[CreateAssetMenu(fileName = "RandomEffectDefinition", menuName = "Pinball/Ball/Effects/Random")]
public sealed class RandomEffectDefinitionSO : BallEffectDefinitionSO
{
    [SerializeField, Range(0f, 0.95f)] private float baseDamageDecreaseRatio = 0.3f;
    [SerializeField, Min(0f)] private float baseDamageIncreaseRatio = 0.3f;
    [SerializeField, Range(0f, 0.95f)] private float baseSpeedDecreaseRatio = 0.3f;
    [SerializeField, Min(0f)] private float baseSpeedIncreaseRatio = 0.3f;
    [SerializeField, Range(0f, 89f)] private float baseAngleVariance = 25f;

    public override BallEffectType EffectType => BallEffectType.Randomize;
    public float BaseDamageDecreaseRatio => baseDamageDecreaseRatio;
    public float BaseDamageIncreaseRatio => baseDamageIncreaseRatio;
    public float BaseSpeedDecreaseRatio => baseSpeedDecreaseRatio;
    public float BaseSpeedIncreaseRatio => baseSpeedIncreaseRatio;
    public float BaseAngleVariance => baseAngleVariance;

    public override BallEffectRuntimeStat CreateRuntimeStat()
    {
        return new RandomEffectRuntimeStat(this);
    }
}
