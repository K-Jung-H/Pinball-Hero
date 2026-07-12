using UnityEngine;

[CreateAssetMenu(fileName = "ActiveBallSkillDefinition", menuName = "Pinball/Skill/Active Ball Definition")]
public sealed class ActiveBallSkillDefinitionSO : SkillDefinitionSO
{
    [SerializeField] private BallType targetBallType = BallType.None;
    [SerializeField] private BallCardDefinitionSO ballCardDefinition;

    public override SkillCategory Category => SkillCategory.ActiveBall;
    public BallType TargetBallType => targetBallType;
    public BallCardDefinitionSO BallCardDefinition => ballCardDefinition;

    protected override SkillDescriptionValue GetDescriptionValue(int level)
    {
        if (level <= 0 || ballCardDefinition == null)
        {
            return SkillDescriptionValue.Zero;
        }

        return ballCardDefinition.GetDescriptionValue(level);
    }
}
