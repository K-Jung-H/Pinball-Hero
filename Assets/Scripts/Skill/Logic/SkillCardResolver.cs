using System.Collections.Generic;
using UnityEngine;

public sealed class SkillCardResolver
{
    private readonly List<SkillCardOption> candidates = new List<SkillCardOption>(16);

    public int Resolve(
        SkillDefinitionSO[] skillPool,
        RunSkillInventory inventory,
        SkillCardOption[] results)
    {
        if (skillPool == null || inventory == null || results == null)
        {
            return 0;
        }

        candidates.Clear();

        for (int i = 0; i < skillPool.Length; i++)
        {
            SkillDefinitionSO definition = skillPool[i];

            if (definition == null)
            {
                continue;
            }

            int currentLevel = inventory.GetSkillLevel(definition);

            if (currentLevel >= definition.MaxLevel)
            {
                continue;
            }

            if (currentLevel <= 0 && !inventory.CanAddNew(definition.Category))
            {
                continue;
            }

            candidates.Add(new SkillCardOption(definition, currentLevel, currentLevel + 1));
        }

        int count = Mathf.Min(results.Length, candidates.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(i, candidates.Count);
            SkillCardOption selected = candidates[randomIndex];
            candidates[randomIndex] = candidates[i];
            candidates[i] = selected;
            results[i] = selected;
        }

        for (int i = count; i < results.Length; i++)
        {
            results[i] = SkillCardOption.Empty;
        }

        return count;
    }
}

public readonly struct SkillCardOption
{
    public static SkillCardOption Empty => default;

    public SkillCardOption(SkillDefinitionSO definition, int currentLevel, int targetLevel)
    {
        Definition = definition;
        CurrentLevel = Mathf.Max(0, currentLevel);
        TargetLevel = Mathf.Max(1, targetLevel);
    }

    public SkillDefinitionSO Definition { get; }
    public int CurrentLevel { get; }
    public int TargetLevel { get; }
    public bool IsValid => Definition != null;
}
