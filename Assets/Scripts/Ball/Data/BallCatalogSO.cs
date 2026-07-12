using UnityEngine;

[CreateAssetMenu(fileName = "BallCatalog", menuName = "Pinball/Ball/Catalog")]
public class BallCatalogSO : ScriptableObject
{
    [SerializeField] private BallDefinitionSO[] ballDefinitions;
    [SerializeField] private BallGrowthDefinitionSO[] growthDefinitions;

    public bool TryGetBallDefinition(BallType type, out BallDefinitionSO definition)
    {
        definition = null;

        if (ballDefinitions == null)
        {
            return false;
        }

        for (int i = 0; i < ballDefinitions.Length; i++)
        {
            if (ballDefinitions[i] != null && ballDefinitions[i].BallType == type)
            {
                definition = ballDefinitions[i];
                return true;
            }
        }

        return false;
    }

    public bool TryGetGrowthDefinition(BallType type, out BallGrowthDefinitionSO definition)
    {
        definition = null;

        if (growthDefinitions == null)
        {
            return false;
        }

        for (int i = 0; i < growthDefinitions.Length; i++)
        {
            if (growthDefinitions[i] != null && growthDefinitions[i].BallType == type)
            {
                definition = growthDefinitions[i];
                return true;
            }
        }

        return false;
    }
}
