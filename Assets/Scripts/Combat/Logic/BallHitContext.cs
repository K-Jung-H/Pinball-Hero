using UnityEngine;

public readonly struct BallHitContext
{
    public BallHitContext(
        Ball_Base ball,
        Enemy_Base enemy,
        Vector2 hitPoint,
        int wallHitCount)
    {
        Ball = ball;
        Enemy = enemy;
        HitPoint = hitPoint;
        TargetCenter = enemy != null ? enemy.Center : hitPoint;
        HitSide = enemy != null
            ? enemy.GetHitSide(hitPoint)
            : EnemyHitSide.None;
        WallHitCount = Mathf.Max(0, wallHitCount);
    }

    public Ball_Base Ball { get; }
    public Enemy_Base Enemy { get; }
    public Vector2 HitPoint { get; }
    public Vector2 TargetCenter { get; }
    public EnemyHitSide HitSide { get; }
    public int WallHitCount { get; }
    public BallType BallType => Ball != null ? Ball.BallType : BallType.None;
}
