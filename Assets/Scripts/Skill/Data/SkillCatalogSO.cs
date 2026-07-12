using UnityEngine;

[CreateAssetMenu(fileName = "SkillCatalog", menuName = "Pinball/Skill/Catalog")]
public class SkillCatalogSO : ScriptableObject
{
    [SerializeField] private SkillDefinitionSO[] skills;

    public SkillDefinitionSO[] Skills => skills;
}
