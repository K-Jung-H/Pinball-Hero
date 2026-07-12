using System;
using System.Collections.Generic;
using UnityEngine;

public class DamageTextSystem : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private DamageTextSprite prefab;
    [SerializeField] private Transform container;
    [SerializeField] private int initialPoolSize = 32;
    [SerializeField] private bool allowPoolExpand;

    [Header("Spawn")]
    [SerializeField] private int maxSpawnPerFrame = 8;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.35f, 0f);

    [Header("Profiles")]
    [SerializeField] private DamageTextProfile[] profiles =
    {
        DamageTextProfile.CreateNormal()
    };

    private readonly Queue<DamageResult> pendingResults = new Queue<DamageResult>(64);
    private readonly Queue<DamageTextSprite> availableTexts = new Queue<DamageTextSprite>(64);

    private CombatPipeline combatPipeline;

    private void Awake()
    {
        if (container == null)
        {
            container = transform;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            DamageTextSprite damageText = CreateInstance();

            if (damageText != null)
            {
                Return(damageText);
            }
        }
    }

    private void OnDisable()
    {
        if (combatPipeline != null)
        {
            combatPipeline.HitResolved -= Enqueue;
            combatPipeline = null;
        }

        pendingResults.Clear();
    }

    private void Update()
    {
        int spawnCount = Mathf.Min(maxSpawnPerFrame, pendingResults.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            DamageTextSprite damageText = Get();

            if (damageText == null)
            {
                return;
            }

            DamageResult result = pendingResults.Dequeue();
            DamageTextProfile profile = GetProfile(ResolveFeedbackType(result));
            damageText.Play(result.Damage, result.HitPoint + (Vector2)worldOffset, profile, this);
        }
    }

    public void SetCombatPipeline(CombatPipeline pipeline)
    {
        if (combatPipeline != null)
        {
            combatPipeline.HitResolved -= Enqueue;
        }

        combatPipeline = pipeline;

        if (combatPipeline != null)
        {
            combatPipeline.HitResolved += Enqueue;
        }
    }

    public void Return(DamageTextSprite damageText)
    {
        if (damageText == null)
        {
            return;
        }

        damageText.gameObject.SetActive(false);
        availableTexts.Enqueue(damageText);
    }

    private void Enqueue(DamageResult result)
    {
        if (result.Damage <= 0)
        {
            return;
        }

        pendingResults.Enqueue(result);
    }

    private DamageTextSprite Get()
    {
        if (availableTexts.Count > 0)
        {
            return availableTexts.Dequeue();
        }

        if (!allowPoolExpand)
        {
            return null;
        }

        return CreateInstance();
    }

    private DamageTextSprite CreateInstance()
    {
        if (prefab == null)
        {
            return null;
        }

        DamageTextSprite damageText = Instantiate(prefab, container);
        damageText.gameObject.SetActive(false);
        return damageText;
    }

    private DamageTextProfile GetProfile(DamageFeedbackType feedbackType)
    {
        if (profiles == null)
        {
            return DamageTextProfile.Normal;
        }

        for (int i = 0; i < profiles.Length; i++)
        {
            DamageTextProfile profile = profiles[i];

            if (profile != null && profile.FeedbackType == feedbackType)
            {
                return profile;
            }
        }

        for (int i = 0; i < profiles.Length; i++)
        {
            if (profiles[i] != null)
            {
                return profiles[i];
            }
        }

        return DamageTextProfile.Normal;
    }

    private static DamageFeedbackType ResolveFeedbackType(DamageResult result)
    {
        if (result.IsCritical)
        {
            return DamageFeedbackType.Critical;
        }

        if (result.IsDot && result.DamageType == DamageType.Fire)
        {
            return DamageFeedbackType.FireDot;
        }

        switch (result.DamageType)
        {
            case DamageType.Fire:
                return DamageFeedbackType.Fire;
            case DamageType.Ice:
                return DamageFeedbackType.Ice;
            case DamageType.Laser:
                return DamageFeedbackType.Laser;
            case DamageType.Explosion:
                return DamageFeedbackType.Explosion;
            default:
                return DamageFeedbackType.Normal;
        }
    }
}

public enum DamageFeedbackType
{
    Normal = 0,
    Critical = 1,
    Fire = 2,
    FireDot = 3,
    Ice = 4,
    Laser = 5,
    Explosion = 6
}

[Serializable]
public sealed class DamageTextProfile
{
    [SerializeField] private DamageFeedbackType feedbackType = DamageFeedbackType.Normal;
    [SerializeField] private Color color = Color.white;
    [SerializeField] private float duration = 0.65f;
    [SerializeField] private float riseSpeed = 1.5f;
    [SerializeField] private AnimationCurve scaleCurve = new AnimationCurve(
        new Keyframe(0f, 0.65f),
        new Keyframe(0.18f, 1.25f),
        new Keyframe(1f, 1f));
    [SerializeField] private AnimationCurve alphaCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(0.65f, 1f),
        new Keyframe(1f, 0f));

    public DamageFeedbackType FeedbackType => feedbackType;
    public Color Color => color;
    public float Duration => duration;
    public float RiseSpeed => riseSpeed;
    public static DamageTextProfile Normal { get; } = CreateNormal();

    public static DamageTextProfile CreateNormal()
    {
        return new DamageTextProfile
        {
            feedbackType = DamageFeedbackType.Normal,
            color = Color.white,
            duration = 0.65f,
            riseSpeed = 1.5f,
            scaleCurve = new AnimationCurve(
                new Keyframe(0f, 0.65f),
                new Keyframe(0.18f, 1.25f),
                new Keyframe(1f, 1f)),
            alphaCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.65f, 1f),
                new Keyframe(1f, 0f))
        };
    }

    public float EvaluateScale(float normalizedTime)
    {
        if (scaleCurve == null)
        {
            return 1f;
        }

        return scaleCurve.Evaluate(normalizedTime);
    }

    public float EvaluateAlpha(float normalizedTime)
    {
        if (alphaCurve == null)
        {
            return 1f;
        }

        return Mathf.Clamp01(alphaCurve.Evaluate(normalizedTime));
    }
}
