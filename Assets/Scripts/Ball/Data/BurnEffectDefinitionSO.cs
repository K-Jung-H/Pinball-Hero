using UnityEngine;

[CreateAssetMenu(fileName = "BurnEffectDefinition", menuName = "Pinball/Ball/Effects/Burn")]
public sealed class BurnEffectDefinitionSO : BallEffectDefinitionSO
{
    [SerializeField] private int baseDotDamage;
    [SerializeField] private float baseDuration;
    [SerializeField] private int baseMaxStack = 1;

    public override BallEffectType EffectType => BallEffectType.Burn;
    public int BaseDotDamage => baseDotDamage;
    public float BaseDuration => baseDuration;
    public int BaseMaxStack => baseMaxStack;

    public override BallEffectRuntimeStat CreateRuntimeStat()
    {
        return new BurnEffectRuntimeStat(this);
    }
}
