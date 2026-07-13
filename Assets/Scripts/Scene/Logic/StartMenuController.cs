using UnityEngine;

public sealed class StartMenuController : MonoBehaviour
{
    [SerializeField] private BallGrowthLevelSelector[] growthSelectors;
    [SerializeField] private SceneNavigator sceneNavigator;

    private void Start()
    {
        if (growthSelectors == null)
        {
            return;
        }

        for (int i = 0; i < growthSelectors.Length; i++)
        {
            BallGrowthLevelSelector selector = growthSelectors[i];

            if (selector == null)
            {
                continue;
            }

            int preferredLevel = 0;

            if (PlayerProgressSession.TryGetBallProgress(
                selector.BallType,
                out PlayerBallProgress progress))
            {
                preferredLevel = progress.UpgradeLevel;
            }

            selector.Initialize(preferredLevel);
        }
    }

    public void Play()
    {
        if (sceneNavigator == null)
        {
            Debug.LogError("SceneNavigator is not assigned.");
            return;
        }

        if (growthSelectors == null || growthSelectors.Length <= 0)
        {
            Debug.LogError("Ball growth selectors are not assigned.");
            return;
        }

        PlayerProgressData progressData = new PlayerProgressData();

        for (int i = 0; i < growthSelectors.Length; i++)
        {
            BallGrowthLevelSelector selector = growthSelectors[i];

            if (selector == null || !selector.IsConfigured)
            {
                Debug.LogError($"Ball growth selector is not configured. Index: {i}");
                return;
            }

            progressData.SetBallProgress(
                selector.BallType,
                selector.SelectedLevel,
                true);
        }

        PlayerProgressSession.SetProgress(progressData);
        sceneNavigator.LoadGameScene();
    }
}
