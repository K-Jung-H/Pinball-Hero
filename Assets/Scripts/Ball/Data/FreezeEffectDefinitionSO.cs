using UnityEngine;

[CreateAssetMenu(fileName = "FreezeEffectDefinition", menuName = "Pinball/Ball/Effects/Freeze")]
public sealed class FreezeEffectDefinitionSO : BallEffectDefinitionSO
{
    [SerializeField] private float baseChance;
    [SerializeField] private float baseDuration;
    [SerializeField] private float baseSlowRatio;
    [SerializeField] private float baseExtraDamageRatio;

    public override BallEffectType EffectType => BallEffectType.Freeze;
    public float BaseChance => baseChance;
    public float BaseDuration => baseDuration;
    public float BaseSlowRatio => baseSlowRatio;
    public float BaseExtraDamageRatio => baseExtraDamageRatio;

    public override BallEffectRuntimeStat CreateRuntimeStat()
    {
        return new FreezeEffectRuntimeStat(this);
    }
}
