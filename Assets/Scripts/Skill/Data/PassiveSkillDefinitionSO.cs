using UnityEngine;

public abstract class PassiveSkillDefinitionSO : SkillDefinitionSO
{
    public sealed override SkillCategory Category => SkillCategory.Passive;

    public virtual float GetDamageBonusRatio(int level, DamageRequest request)
    {
        return 0f;
    }

    public virtual float GetCritChanceBonus(int level, DamageRequest request)
    {
        return 0f;
    }
}
