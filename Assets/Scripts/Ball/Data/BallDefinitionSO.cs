using UnityEngine;

[CreateAssetMenu(fileName = "BallDefinition", menuName = "Pinball/Ball/Definition")]
public class BallDefinitionSO : ScriptableObject
{
    [SerializeField] private BallType ballType = BallType.Normal;
    [SerializeField] private Ball_Base prefab;
    [SerializeField] private DamageType damageType = DamageType.Normal;
    [SerializeField] private int baseHitDamage = 1;
    [SerializeField] private float baseMoveSpeed = 20f;
    [SerializeField] private float baseCritChance;
    [SerializeField] private float baseCritDamageMultiplier = 1.5f;
    [SerializeField] private BallEffectDefinitionSO[] effects;

    public BallType BallType => ballType;
    public Ball_Base Prefab => prefab;
    public DamageType DamageType => damageType;
    public int BaseHitDamage => baseHitDamage;
    public float BaseMoveSpeed => baseMoveSpeed;
    public float BaseCritChance => baseCritChance;
    public float BaseCritDamageMultiplier => baseCritDamageMultiplier;
    public BallEffectDefinitionSO[] Effects => effects;
}
