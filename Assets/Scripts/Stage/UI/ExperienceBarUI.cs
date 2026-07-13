using UnityEngine;
using UnityEngine.UI;

public sealed class ExperienceBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [Min(0.01f)]
    [SerializeField] private float lerpSpeed = 8f;

    private StageExperienceSystem experienceSystem;
    private float targetFillAmount;
    private int pendingLevelUpAnimations;

    private void Update()
    {
        if (fillImage == null)
        {
            return;
        }

        float target = pendingLevelUpAnimations > 0
            ? 1f
            : targetFillAmount;
        float lerpRatio = 1f - Mathf.Exp(
            -Mathf.Max(0.01f, lerpSpeed) * Time.unscaledDeltaTime);

        fillImage.fillAmount = Mathf.Lerp(
            fillImage.fillAmount,
            target,
            lerpRatio);

        if (Mathf.Abs(fillImage.fillAmount - target) > 0.001f)
        {
            return;
        }

        fillImage.fillAmount = target;

        if (pendingLevelUpAnimations > 0)
        {
            pendingLevelUpAnimations--;
            fillImage.fillAmount = 0f;
        }
    }

    public void Bind(StageExperienceSystem system)
    {
        if (experienceSystem != null)
        {
            experienceSystem.ProgressChanged -= OnProgressChanged;
        }

        experienceSystem = system;
        pendingLevelUpAnimations = 0;
        targetFillAmount = experienceSystem != null
            ? experienceSystem.NormalizedProgress
            : 0f;

        if (fillImage != null)
        {
            fillImage.fillAmount = targetFillAmount;
        }

        if (experienceSystem != null)
        {
            experienceSystem.ProgressChanged += OnProgressChanged;
        }
    }

    private void OnProgressChanged(
        float normalizedProgress,
        int completedLevelCount,
        bool isMaxLevel)
    {
        int resetAnimationCount = isMaxLevel
            ? Mathf.Max(0, completedLevelCount - 1)
            : Mathf.Max(0, completedLevelCount);

        pendingLevelUpAnimations += resetAnimationCount;
        targetFillAmount = Mathf.Clamp01(normalizedProgress);
    }

    private void OnDestroy()
    {
        Bind(null);
    }
}
