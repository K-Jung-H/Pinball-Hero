using System;
using System.Globalization;
using UnityEngine;

public abstract class SkillDefinitionSO : ScriptableObject
{
    [SerializeField] private string skillId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private int maxLevel = 1;
    [TextArea]
    [SerializeField] private string descriptionFormat;

    public string SkillId => skillId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public Sprite Icon => icon;
    public int MaxLevel => Mathf.Max(1, maxLevel);
    public abstract SkillCategory Category { get; }

    public string BuildDescription(int currentLevel, int targetLevel)
    {
        if (string.IsNullOrEmpty(descriptionFormat))
        {
            return string.Empty;
        }

        SkillDescriptionValue currentValue = GetDescriptionValue(currentLevel);
        SkillDescriptionValue targetValue = GetDescriptionValue(targetLevel);
        SkillDescriptionValue deltaValue = targetValue - currentValue;

        string text = descriptionFormat;
        text = text.Replace("{level}", targetLevel.ToString(CultureInfo.InvariantCulture));
        text = text.Replace("{hitDamage}", FormatNumber(targetValue.HitDamage));
        text = text.Replace("{hitDamageDelta}", FormatSignedNumber(deltaValue.HitDamage));
        text = text.Replace("{damage}", FormatNumber(targetValue.EffectDamage));
        text = text.Replace("{damageDelta}", FormatSignedNumber(deltaValue.EffectDamage));
        text = text.Replace("{chance}", FormatNumber(targetValue.ChancePercent));
        text = text.Replace("{chanceDelta}", FormatSignedNumber(deltaValue.ChancePercent));
        text = text.Replace("{duration}", FormatNumber(targetValue.Duration));
        text = text.Replace("{durationDelta}", FormatSignedNumber(deltaValue.Duration));
        text = text.Replace("{maxStack}", FormatNumber(targetValue.MaxStack));
        text = text.Replace("{maxStackDelta}", FormatSignedNumber(deltaValue.MaxStack));
        text = text.Replace("{slow}", FormatNumber(targetValue.SlowPercent));
        text = text.Replace("{slowDelta}", FormatSignedNumber(deltaValue.SlowPercent));
        text = text.Replace("{extraDamage}", FormatNumber(targetValue.ExtraDamagePercent));
        text = text.Replace("{extraDamageDelta}", FormatSignedNumber(deltaValue.ExtraDamagePercent));
        text = text.Replace("{damageMultiplier}", FormatNumber(targetValue.DamageMultiplierPercent));
        text = text.Replace("{damageMultiplierDelta}", FormatSignedNumber(deltaValue.DamageMultiplierPercent));
        text = text.Replace("{damageAdd}", FormatNumber(targetValue.PassiveDamageAdd));
        text = text.Replace("{damageAddDelta}", FormatSignedNumber(deltaValue.PassiveDamageAdd));
        return text;
    }

    protected abstract SkillDescriptionValue GetDescriptionValue(int level);

    public static float ToPercent(float value)
    {
        return value * 100f;
    }

    private static string FormatNumber(float value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string FormatSignedNumber(float value)
    {
        if (Mathf.Approximately(value, 0f))
        {
            return "0";
        }

        return value > 0f
            ? "+" + FormatNumber(value)
            : FormatNumber(value);
    }
}

public struct SkillDescriptionValue
{
    public static SkillDescriptionValue Zero => default;

    public float HitDamage;
    public float EffectDamage;
    public float ChancePercent;
    public float Duration;
    public float MaxStack;
    public float SlowPercent;
    public float ExtraDamagePercent;
    public float DamageMultiplierPercent;
    public float PassiveDamageAdd;

    public static SkillDescriptionValue operator -(SkillDescriptionValue left, SkillDescriptionValue right)
    {
        return new SkillDescriptionValue
        {
            HitDamage = left.HitDamage - right.HitDamage,
            EffectDamage = left.EffectDamage - right.EffectDamage,
            ChancePercent = left.ChancePercent - right.ChancePercent,
            Duration = left.Duration - right.Duration,
            MaxStack = left.MaxStack - right.MaxStack,
            SlowPercent = left.SlowPercent - right.SlowPercent,
            ExtraDamagePercent = left.ExtraDamagePercent - right.ExtraDamagePercent,
            DamageMultiplierPercent = left.DamageMultiplierPercent - right.DamageMultiplierPercent,
            PassiveDamageAdd = left.PassiveDamageAdd - right.PassiveDamageAdd
        };
    }
}
