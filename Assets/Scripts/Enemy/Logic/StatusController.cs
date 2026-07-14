using System;
using UnityEngine;

public enum EnemyVisualStatus
{
    Idle = 0,
    Burn = 1,
    Frozen = 2
}

public sealed class StatusController : MonoBehaviour
{
    private const float BurnTickInterval = 1f;

    private Enemy_Base owner;
    private CombatPipeline combatPipeline;

    private int burnStackCount;
    private int burnMaxStack;
    private int burnTickDamage;
    private float burnRemainingTime;
    private float burnTickTimer;

    private float freezeRemainingTime;
    private float freezeSlowRatio;
    private float freezeExtraDamageRatio;
    private EnemyVisualStatus visualStatus;

    private bool IsBurning => burnStackCount > 0;
    private bool IsFrozen => freezeRemainingTime > 0f;
    public EnemyVisualStatus VisualStatus => visualStatus;

    public event Action<EnemyVisualStatus> VisualStatusChanged;

    public float MoveSpeedMultiplier => freezeRemainingTime > 0f
        ? Mathf.Clamp01(1f - freezeSlowRatio)
        : 1f;

    public float IncomingDamageMultiplier => freezeRemainingTime > 0f
        ? 1f + freezeExtraDamageRatio
        : 1f;

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        UpdateBurn(deltaTime);
        UpdateFreeze(deltaTime);
        RefreshVisualStatus();

        if (burnStackCount <= 0 && freezeRemainingTime <= 0f)
        {
            enabled = false;
        }
    }

    public void Initialize(Enemy_Base target)
    {
        owner = target;
        ResetState();
    }

    public void ApplyBurn(
        int dotDamage,
        float duration,
        int maxStack,
        CombatPipeline pipeline)
    {
        if (owner == null
            || !owner.CanReceiveDamage
            || pipeline == null
            || dotDamage <= 0
            || duration <= 0f
            || maxStack <= 0)
        {
            return;
        }

        combatPipeline = pipeline;
        burnMaxStack = Mathf.Max(burnMaxStack, maxStack);

        if (burnStackCount < burnMaxStack)
        {
            burnStackCount++;
            burnTickDamage += dotDamage;
        }

        burnRemainingTime = duration;

        if (burnTickTimer <= 0f)
        {
            burnTickTimer = BurnTickInterval;
        }

        SetVisualStatus(EnemyVisualStatus.Burn);
        enabled = true;
    }

    public void ApplyFreeze(
        float duration,
        float slowRatio,
        float extraDamageRatio)
    {
        if (owner == null
            || !owner.CanReceiveDamage
            || duration <= 0f)
        {
            return;
        }

        freezeRemainingTime = Mathf.Max(freezeRemainingTime, duration);
        freezeSlowRatio = Mathf.Max(freezeSlowRatio, Mathf.Clamp01(slowRatio));
        freezeExtraDamageRatio = Mathf.Max(freezeExtraDamageRatio, Mathf.Max(0f, extraDamageRatio));
        SetVisualStatus(EnemyVisualStatus.Frozen);
        enabled = true;
    }

    public void ResetState()
    {
        combatPipeline = null;

        burnStackCount = 0;
        burnMaxStack = 0;
        burnTickDamage = 0;
        burnRemainingTime = 0f;
        burnTickTimer = 0f;

        freezeRemainingTime = 0f;
        freezeSlowRatio = 0f;
        freezeExtraDamageRatio = 0f;

        SetVisualStatus(EnemyVisualStatus.Idle);
        enabled = false;
    }

    private void UpdateBurn(float deltaTime)
    {
        if (burnStackCount <= 0)
        {
            return;
        }

        float activeTime = Mathf.Min(deltaTime, burnRemainingTime);
        burnRemainingTime -= deltaTime;
        burnTickTimer -= activeTime;

        while (burnTickTimer <= 0f && burnStackCount > 0)
        {
            ResolveBurnDamage();
            burnTickTimer += BurnTickInterval;

            if (owner == null || !owner.CanReceiveDamage)
            {
                break;
            }
        }

        if (burnRemainingTime <= 0f)
        {
            burnStackCount = 0;
            burnMaxStack = 0;
            burnTickDamage = 0;
            burnRemainingTime = 0f;
            burnTickTimer = 0f;
        }
    }

    private void UpdateFreeze(float deltaTime)
    {
        if (freezeRemainingTime <= 0f)
        {
            return;
        }

        freezeRemainingTime -= deltaTime;

        if (freezeRemainingTime > 0f)
        {
            return;
        }

        freezeRemainingTime = 0f;
        freezeSlowRatio = 0f;
        freezeExtraDamageRatio = 0f;
    }

    private void ResolveBurnDamage()
    {
        if (owner == null
            || !owner.CanReceiveDamage
            || combatPipeline == null
            || burnTickDamage <= 0)
        {
            return;
        }

        DamageRequest request = new DamageRequest(
            burnTickDamage,
            BallType.Fire,
            DamageType.Fire,
            DamageSourceType.Dot,
            false,
            true,
            owner.Center,
            null,
            null,
            owner);

        combatPipeline.ResolveDamage(request);
    }

    private void RefreshVisualStatus()
    {
        if (visualStatus == EnemyVisualStatus.Burn && !IsBurning)
        {
            SetVisualStatus(IsFrozen
                ? EnemyVisualStatus.Frozen
                : EnemyVisualStatus.Idle);
            return;
        }

        if (visualStatus == EnemyVisualStatus.Frozen && !IsFrozen)
        {
            SetVisualStatus(IsBurning
                ? EnemyVisualStatus.Burn
                : EnemyVisualStatus.Idle);
        }
    }

    private void SetVisualStatus(EnemyVisualStatus value)
    {
        if (visualStatus == value)
        {
            return;
        }

        visualStatus = value;
        VisualStatusChanged?.Invoke(visualStatus);
    }
}
