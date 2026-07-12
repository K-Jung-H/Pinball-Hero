using UnityEngine;

[CreateAssetMenu(fileName = "PierceEffectDefinition", menuName = "Pinball/Ball/Effects/Pierce")]
public sealed class PierceEffectDefinitionSO : BallEffectDefinitionSO
{
    public override BallEffectType EffectType => BallEffectType.Pierce;

    public override BallEffectRuntimeStat CreateRuntimeStat()
    {
        return new PierceEffectRuntimeStat(this);
    }
}
