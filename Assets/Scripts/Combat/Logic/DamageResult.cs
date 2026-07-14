using UnityEngine;

public readonly struct DamageResult
{
    public DamageResult(
        int damage,
        DamageType damageType,
        DamageSourceType sourceType,
        bool isCritical,
        bool isDot,
        Vector2 hitPoint,
        Ball_Base ball,
        Enemy_Base enemy)
    {
        Damage = damage;
        DamageType = damageType;
        SourceType = sourceType;
        IsCritical = isCritical;
        IsDot = isDot;
        HitPoint = hitPoint;
        Ball = ball;
        Enemy = enemy;
    }

    public int Damage { get; }
    public DamageType DamageType { get; }
    public DamageSourceType SourceType { get; }
    public bool IsCritical { get; }
    public bool IsDot { get; }
    public Vector2 HitPoint { get; }
    public Ball_Base Ball { get; }
    public Enemy_Base Enemy { get; }
}
