using System;
using UnityEngine;

public sealed class StageExperienceSystem : MonoBehaviour
{
    private StageDefinitionSO stageDefinition;
    private SkillSelectionController skillSelectionController;
    private int stageLevel;
    private int currentExperience;
    private int requiredExperience;
    private int pendingSelectionCount;
    private bool isInitialized;

    public int StageLevel => stageLevel;
    public int CurrentExperience => currentExperience;
    public int RequiredExperience => requiredExperience;
    public int PendingSelectionCount => pendingSelectionCount;
    public bool IsMaxLevel => isInitialized
        && stageDefinition != null
        && stageLevel >= stageDefinition.MaxStageLevel;
    public float NormalizedProgress => IsMaxLevel
        ? 1f
        : requiredExperience > 0
            ? Mathf.Clamp01((float)currentExperience / requiredExperience)
            : 0f;

    public event Action<float, int, bool> ProgressChanged;

    public bool Initialize(
        StageDefinitionSO definition,
        SkillSelectionController selectionController)
    {
        UnsubscribeSelectionController();

        stageDefinition = definition;
        skillSelectionController = selectionController;
        stageLevel = 0;
        currentExperience = 0;
        pendingSelectionCount = 0;
        isInitialized = stageDefinition != null
            && skillSelectionController != null
            && stageDefinition.TryGetRequiredExperience(stageLevel, out requiredExperience);

        if (!isInitialized)
        {
            requiredExperience = 0;
            ProgressChanged?.Invoke(0f, 0, false);
            return false;
        }

        skillSelectionController.SelectionCompleted += OnSelectionCompleted;
        ProgressChanged?.Invoke(NormalizedProgress, 0, IsMaxLevel);
        return true;
    }

    public void AddExperience(int amount)
    {
        if (!isInitialized || amount <= 0 || IsMaxLevel)
        {
            return;
        }

        currentExperience = (int)Math.Min(
            int.MaxValue,
            (long)currentExperience + amount);

        int completedLevelCount = 0;

        while (!IsMaxLevel && currentExperience >= requiredExperience)
        {
            currentExperience -= requiredExperience;
            stageLevel++;
            completedLevelCount++;
            pendingSelectionCount++;

            if (IsMaxLevel)
            {
                currentExperience = 0;
                break;
            }

            if (!stageDefinition.TryGetRequiredExperience(stageLevel, out requiredExperience))
            {
                isInitialized = false;
                requiredExperience = 0;
                break;
            }
        }

        ProgressChanged?.Invoke(
            NormalizedProgress,
            completedLevelCount,
            IsMaxLevel);

        TryShowNextSelection();
    }

    private void OnSelectionCompleted()
    {
        TryShowNextSelection();
    }

    private void TryShowNextSelection()
    {
        if (pendingSelectionCount <= 0
            || skillSelectionController == null
            || skillSelectionController.IsOpen)
        {
            return;
        }

        skillSelectionController.ShowChoices();

        if (skillSelectionController.IsOpen)
        {
            pendingSelectionCount--;
        }
        else
        {
            pendingSelectionCount = 0;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeSelectionController();
    }

    private void UnsubscribeSelectionController()
    {
        if (skillSelectionController != null)
        {
            skillSelectionController.SelectionCompleted -= OnSelectionCompleted;
        }
    }
}
