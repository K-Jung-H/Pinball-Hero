using UnityEngine;

public sealed class RandomBall : Ball_Base
{
    private const float MinimumSpeed = 0.1f;
    private const float MinimumOutwardDot = 0.05f;

    private int currentHitDamage;

    public override int CurrentHitDamage => currentHitDamage;

    public override void ResetState()
    {
        base.ResetState();
        currentHitDamage = 0;
    }

    protected override void OnLaunched()
    {
        currentHitDamage = RuntimeStat.HitDamage;
    }

    protected override void OnFlyingCollisionResolved(Collision2D collision)
    {
        RandomEffectRuntimeStat effect = RuntimeStat.GetEffect<RandomEffectRuntimeStat>();

        if (effect == null || BallRigidbody == null)
        {
            return;
        }

        float damageMultiplier = Random.Range(
            1f - effect.DamageDecreaseRatio,
            1f + effect.DamageIncreaseRatio);
        float speedMultiplier = Random.Range(
            1f - effect.SpeedDecreaseRatio,
            1f + effect.SpeedIncreaseRatio);

        currentHitDamage = Mathf.Max(
            1,
            Mathf.RoundToInt(RuntimeStat.HitDamage * damageMultiplier));

        float speed = Mathf.Max(
            MinimumSpeed,
            RuntimeStat.MoveSpeed * speedMultiplier);
        Vector2 direction = BallRigidbody.linearVelocity;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        direction.Normalize();
        Vector2 normal = GetContactNormal(collision);

        if (normal.sqrMagnitude > 0.0001f
            && Vector2.Dot(direction, normal) < 0f)
        {
            direction = Vector2.Reflect(direction, normal);
        }

        float angle = Random.Range(-effect.AngleVariance, effect.AngleVariance);
        direction = Rotate(direction, angle);

        if (normal.sqrMagnitude > 0.0001f)
        {
            float outwardDot = Vector2.Dot(direction, normal);

            if (outwardDot < MinimumOutwardDot)
            {
                direction = (direction
                    + normal * (MinimumOutwardDot - outwardDot)).normalized;
            }
        }

        SetFlightMotion(direction, speed);
    }

    private static Vector2 GetContactNormal(Collision2D collision)
    {
        return collision != null && collision.contactCount > 0
            ? collision.GetContact(0).normal.normalized
            : Vector2.zero;
    }

    private static Vector2 Rotate(Vector2 direction, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos);
    }
}
