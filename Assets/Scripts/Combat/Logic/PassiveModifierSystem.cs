using UnityEngine;

public sealed class PassiveModifierSystem
{
    private RunSkillInventory runSkillInventory;

    public void SetRunSkillInventory(RunSkillInventory inventory)
    {
        runSkillInventory = inventory;
    }

    public DamageResult Apply(DamageRequest request)
    {
        int damage = Mathf.Max(0, request.BaseDamage);
        float damageBonusRatio = 0f;
        float critChance = request.RuntimeStat != null
            ? request.RuntimeStat.CritChance
            : 0f;
        bool isCritical = false;

        if (runSkillInventory != null)
        {
            for (int i = 0; i < runSkillInventory.PassiveSkillCount; i++)
            {
                RunSkillState state = runSkillInventory.GetPassiveSkill(i);

                PassiveSkillDefinitionSO passiveDefinition = state != null
                    ? state.Definition as PassiveSkillDefinitionSO
                    : null;

                if (passiveDefinition == null)
                {
                    continue;
                }

                damageBonusRatio += Mathf.Max(
                    0f,
                    passiveDefinition.GetDamageBonusRatio(state.Level, request));

                critChance += Mathf.Max(
                    0f,
                    passiveDefinition.GetCritChanceBonus(state.Level, request));
            }
        }

        damage = Mathf.Max(
            0,
            Mathf.RoundToInt(damage * (1f + damageBonusRatio)));

        if (request.CanCrit
            && request.RuntimeStat != null
            && Random.value < Mathf.Clamp01(critChance))
        {
            damage = Mathf.RoundToInt(
                damage * request.RuntimeStat.CritDamageMultiplier);
            isCritical = true;
        }

        return new DamageResult(
            damage,
            request.DamageType,
            request.SourceType,
            isCritical,
            request.IsDot,
            request.HitPoint,
            request.Ball,
            request.Enemy);
    }
}
