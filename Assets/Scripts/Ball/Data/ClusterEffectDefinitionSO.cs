using UnityEngine;

[CreateAssetMenu(fileName = "ClusterEffectDefinition", menuName = "Pinball/Ball/Effects/Cluster")]
public sealed class ClusterEffectDefinitionSO : BallEffectDefinitionSO
{
    [SerializeField] private float baseSpawnChance;
    [SerializeField] private int baseSpawnDamage;
    [SerializeField] private BallType spawnedBallType = BallType.ClusterFragment;
    [SerializeField, Range(0f, 89f)] private float minNoiseAngle = 5f;
    [SerializeField, Range(0f, 89f)] private float maxNoiseAngle = 20f;

    public override BallEffectType EffectType => BallEffectType.Cluster;
    public float BaseSpawnChance => baseSpawnChance;
    public int BaseSpawnDamage => baseSpawnDamage;
    public BallType SpawnedBallType => spawnedBallType;
    public float MinNoiseAngle => minNoiseAngle;
    public float MaxNoiseAngle => maxNoiseAngle;

    public override BallEffectRuntimeStat CreateRuntimeStat()
    {
        return new ClusterEffectRuntimeStat(this);
    }
}
