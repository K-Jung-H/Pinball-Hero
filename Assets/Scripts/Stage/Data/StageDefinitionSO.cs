using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StageDefinition", menuName = "Pinball/Stage/Stage Definition")]
public class StageDefinitionSO : ScriptableObject
{
    [SerializeField] private int initialSpawnRows = 3;
    [SerializeField] private WaveDefinitionSO[] waves;

    [Header("Stage Experience")]
    [Min(1)]
    [SerializeField] private int maxStageLevel = 20;
    [SerializeField] private StageExperienceRequirement[] experienceRequirements;

    public int InitialSpawnRows => initialSpawnRows;
    public WaveDefinitionSO[] Waves => waves;
    public int MaxStageLevel => Mathf.Max(1, maxStageLevel);

    public bool TryGetRequiredExperience(int stageLevel, out int requiredExperience)
    {
        requiredExperience = 0;

        if (experienceRequirements == null || experienceRequirements.Length <= 0)
        {
            return false;
        }

        int bestStartLevel = int.MinValue;

        for (int i = 0; i < experienceRequirements.Length; i++)
        {
            StageExperienceRequirement requirement = experienceRequirements[i];

            if (requirement.StartLevel > stageLevel
                || requirement.StartLevel < bestStartLevel)
            {
                continue;
            }

            bestStartLevel = requirement.StartLevel;
            requiredExperience = requirement.RequiredExperience;
        }

        return bestStartLevel != int.MinValue && requiredExperience > 0;
    }
}

[Serializable]
public struct StageExperienceRequirement
{
    [Min(0)]
    [SerializeField] private int startLevel;
    [Min(1)]
    [SerializeField] private int requiredExperience;

    public int StartLevel => Mathf.Max(0, startLevel);
    public int RequiredExperience => Mathf.Max(1, requiredExperience);
}
