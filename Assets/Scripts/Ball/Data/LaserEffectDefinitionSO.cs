using UnityEngine;

[CreateAssetMenu(fileName = "LaserEffectDefinition", menuName = "Pinball/Ball/Effects/Laser")]
public sealed class LaserEffectDefinitionSO : BallEffectDefinitionSO
{
    [SerializeField] private int baseRowDamage;
    [SerializeField] private DamageArea areaPrefab;
    [SerializeField] private int rowHeightInCells = 1;

    public override BallEffectType EffectType => BallEffectType.Laser;
    public int BaseRowDamage => baseRowDamage;
    public DamageArea AreaPrefab => areaPrefab;
    public int RowHeightInCells => rowHeightInCells;

    public override BallEffectRuntimeStat CreateRuntimeStat()
    {
        return new LaserEffectRuntimeStat(this);
    }
}
