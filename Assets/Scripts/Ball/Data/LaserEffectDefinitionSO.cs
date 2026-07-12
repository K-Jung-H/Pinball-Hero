using UnityEngine;

[CreateAssetMenu(fileName = "LaserEffectDefinition", menuName = "Pinball/Ball/Effects/Laser")]
public sealed class LaserEffectDefinitionSO : BallEffectDefinitionSO
{
    [SerializeField] private int baseRowDamage;

    public override BallEffectType EffectType => BallEffectType.Laser;
    public int BaseRowDamage => baseRowDamage;

    public override BallEffectRuntimeStat CreateRuntimeStat()
    {
        return new LaserEffectRuntimeStat(this);
    }
}
