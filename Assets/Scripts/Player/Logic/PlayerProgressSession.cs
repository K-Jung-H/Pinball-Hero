using UnityEngine;

public static class PlayerProgressSession
{
    private static PlayerProgressData progressData;

    public static bool HasProgress => progressData != null
        && progressData.BallProgressCount > 0;

    public static void SetProgress(PlayerProgressData value)
    {
        progressData = value;
    }

    public static bool TryGetBallProgress(
        BallType ballType,
        out PlayerBallProgress progress)
    {
        if (progressData != null)
        {
            return progressData.TryGetBallProgress(ballType, out progress);
        }

        progress = null;
        return false;
    }

    public static StageBallProgress[] CreateStageBallProgresses()
    {
        return progressData != null
            ? progressData.CreateStageBallProgresses()
            : null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        progressData = null;
    }
}
