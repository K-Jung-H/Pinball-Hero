using UnityEngine;

[RequireComponent(typeof(Transform))]
public class BallFactory : MonoBehaviour
{
    [SerializeField] private BallCatalogSO ballCatalog;

    public Ball_Base Create(BallType type, Vector3 position, Transform parent)
    {
        return Create(type, position, parent, null, null);
    }

    public Ball_Base Create(
        BallType type,
        Vector3 position,
        Transform parent,
        StageBallProgress[] stageBallProgresses,
        RunSkillInventory runSkillInventory)
    {
        if (ballCatalog == null)
        {
            Debug.LogError("BallCatalog is not assigned.");
            return null;
        }

        if (!ballCatalog.TryGetBallDefinition(type, out BallDefinitionSO definition))
        {
            Debug.LogError($"Ball definition not found. Type: {type}");
            return null;
        }

        Ball_Base prefab = definition.Prefab;

        if (prefab == null)
        {
            Debug.LogError($"Ball prefab not found. Type: {type}");
            return null;
        }

        Ball_Base ball = Instantiate(prefab, position, Quaternion.identity, parent);
        FillRuntimeStat(type, stageBallProgresses, runSkillInventory, ball.RuntimeStat);
        ball.ApplyRuntimeStat();

        return ball;
    }

    public void FillRuntimeStat(
        BallType type,
        StageBallProgress[] stageBallProgresses,
        RunSkillInventory runSkillInventory,
        BallRuntimeStat runtimeStat)
    {
        if (runtimeStat == null)
        {
            return;
        }

        if (ballCatalog == null || !ballCatalog.TryGetBallDefinition(type, out BallDefinitionSO definition))
        {
            runtimeStat.SetBase(null);
            return;
        }

        runtimeStat.SetBase(definition);

        if (runSkillInventory != null
            && runSkillInventory.TryGetActiveBallSkill(type, out ActiveBallSkillDefinitionSO skillDefinition, out int cardLevel)
            && skillDefinition.BallCardDefinition != null
            && skillDefinition.BallCardDefinition.BallType == type)
        {
            skillDefinition.BallCardDefinition.ApplyLevel(cardLevel, runtimeStat);
        }

        int upgradeLevel = GetUpgradeLevel(type, stageBallProgresses);

        if (ballCatalog.TryGetGrowthDefinition(type, out BallGrowthDefinitionSO growthDefinition)
            && growthDefinition.BallType == type)
        {
            growthDefinition.ApplyLevel(upgradeLevel, runtimeStat);
        }
    }

    public void FillSpawnedBallRuntimeStat(
        BallType type,
        int hitDamage,
        BallRuntimeStat runtimeStat)
    {
        if (runtimeStat == null)
        {
            return;
        }

        if (ballCatalog == null || !ballCatalog.TryGetBallDefinition(type, out BallDefinitionSO definition))
        {
            runtimeStat.SetBase(null);
            return;
        }

        runtimeStat.SetBase(definition);
        runtimeStat.SetHitDamage(hitDamage);
    }

    private static int GetUpgradeLevel(BallType type, StageBallProgress[] stageBallProgresses)
    {
        if (stageBallProgresses == null)
        {
            return 0;
        }

        for (int i = 0; i < stageBallProgresses.Length; i++)
        {
            StageBallProgress progress = stageBallProgresses[i];

            if (progress != null && progress.BallType == type)
            {
                return progress.UpgradeLevel;
            }
        }

        return 0;
    }
}
