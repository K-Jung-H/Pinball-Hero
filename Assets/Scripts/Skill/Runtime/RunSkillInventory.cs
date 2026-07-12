using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class RunSkillInventory
{
    [SerializeField] private int maxActiveSkillCount = 4;
    [SerializeField] private int maxPassiveSkillCount = 2;
    [SerializeField] private List<RunSkillState> activeSkills = new List<RunSkillState>(4);
    [SerializeField] private List<RunSkillState> passiveSkills = new List<RunSkillState>(2);

    public int ActiveSkillCount => activeSkills.Count;
    public int PassiveSkillCount => passiveSkills.Count;
    public int MaxActiveSkillCount => Mathf.Max(0, maxActiveSkillCount);
    public int MaxPassiveSkillCount => Mathf.Max(0, maxPassiveSkillCount);

    public int GetSkillLevel(SkillDefinitionSO definition)
    {
        if (definition == null)
        {
            return 0;
        }

        RunSkillState state = FindState(GetList(definition.Category), definition);
        return state != null ? state.Level : 0;
    }

    public bool CanAddNew(SkillCategory category)
    {
        List<RunSkillState> list = GetList(category);
        int maxCount = category == SkillCategory.ActiveBall
            ? MaxActiveSkillCount
            : MaxPassiveSkillCount;

        return list.Count < maxCount;
    }

    public RunSkillState GetPassiveSkill(int index)
    {
        if (index < 0 || index >= passiveSkills.Count)
        {
            return null;
        }

        return passiveSkills[index];
    }

    public int GetActiveSkillLevelForBall(BallType ballType)
    {
        if (TryGetActiveBallSkill(ballType, out _, out int level))
        {
            return level;
        }

        return 0;
    }

    public bool TryGetActiveBallSkill(
        BallType ballType,
        out ActiveBallSkillDefinitionSO definition,
        out int level)
    {
        definition = null;
        level = 0;

        for (int i = 0; i < activeSkills.Count; i++)
        {
            RunSkillState state = activeSkills[i];
            ActiveBallSkillDefinitionSO activeDefinition = state != null
                ? state.Definition as ActiveBallSkillDefinitionSO
                : null;

            if (activeDefinition != null && activeDefinition.TargetBallType == ballType)
            {
                definition = activeDefinition;
                level = state.Level;
                return true;
            }
        }

        return false;
    }

    public bool AddOrUpgrade(SkillDefinitionSO definition)
    {
        if (definition == null)
        {
            return false;
        }

        List<RunSkillState> list = GetList(definition.Category);

        int maxCount = definition.Category == SkillCategory.ActiveBall
            ? MaxActiveSkillCount
            : MaxPassiveSkillCount;

        RunSkillState state = FindState(list, definition);

        if (state != null)
        {
            return state.TryLevelUp(definition.MaxLevel);
        }

        if (list.Count >= maxCount)
        {
            return false;
        }

        list.Add(new RunSkillState(definition, 1));
        return true;
    }

    private List<RunSkillState> GetList(SkillCategory category)
    {
        return category == SkillCategory.ActiveBall
            ? activeSkills
            : passiveSkills;
    }

    private static RunSkillState FindState(List<RunSkillState> list, SkillDefinitionSO definition)
    {
        for (int i = 0; i < list.Count; i++)
        {
            RunSkillState state = list[i];

            if (state != null && state.Definition == definition)
            {
                return state;
            }
        }

        return null;
    }
}

[Serializable]
public sealed class RunSkillState
{
    [SerializeField] private SkillDefinitionSO definition;
    [SerializeField] private int level;

    public RunSkillState(SkillDefinitionSO definition, int level)
    {
        this.definition = definition;
        this.level = Mathf.Max(1, level);
    }

    public SkillDefinitionSO Definition => definition;
    public int Level => Mathf.Max(0, level);

    public bool TryLevelUp(int maxLevel)
    {
        if (level >= maxLevel)
        {
            return false;
        }

        level++;
        return true;
    }
}
