using UnityEngine;

[CreateAssetMenu(fileName = "ClusterEffectDefinition", menuName = "Pinball/Ball/Effects/Cluster")]
public sealed class ClusterEffectDefinitionSO : BallEffectDefinitionSO
{
    [SerializeField] private float baseSpawnChance;
    [SerializeField] private int baseSpawnDamage;
    [SerializeField] private BallType spawnedBallType = BallType.ClusterFragment;

    public override BallEffectType EffectType => BallEffectType.Cluster;
    public float BaseSpawnChance => baseSpawnChance;
    public int BaseSpawnDamage => baseSpawnDamage;
    public BallType SpawnedBallType => spawnedBallType;

    public override BallEffectRuntimeStat CreateRuntimeStat()
    {
        return new ClusterEffectRuntimeStat(this);
    }
}
