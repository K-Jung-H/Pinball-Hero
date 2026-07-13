using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class PlayerProgressData
{
    [SerializeField] private List<PlayerBallProgress> ballProgresses =
        new List<PlayerBallProgress>(6);

    public int BallProgressCount => ballProgresses.Count;

    public void SetBallProgress(BallType ballType, int upgradeLevel, bool unlocked)
    {
        if (ballType == BallType.None)
        {
            return;
        }

        PlayerBallProgress progress = Find(ballType);

        if (progress != null)
        {
            progress.Set(upgradeLevel, unlocked);
            return;
        }

        ballProgresses.Add(new PlayerBallProgress(ballType, upgradeLevel, unlocked));
    }

    public bool TryGetBallProgress(BallType ballType, out PlayerBallProgress progress)
    {
        progress = Find(ballType);
        return progress != null;
    }

    public StageBallProgress[] CreateStageBallProgresses()
    {
        StageBallProgress[] result = new StageBallProgress[ballProgresses.Count];

        for (int i = 0; i < ballProgresses.Count; i++)
        {
            PlayerBallProgress progress = ballProgresses[i];
            result[i] = new StageBallProgress(
                progress.BallType,
                progress.UpgradeLevel,
                progress.Unlocked);
        }

        return result;
    }

    private PlayerBallProgress Find(BallType ballType)
    {
        for (int i = 0; i < ballProgresses.Count; i++)
        {
            PlayerBallProgress progress = ballProgresses[i];

            if (progress != null && progress.BallType == ballType)
            {
                return progress;
            }
        }

        return null;
    }
}

[Serializable]
public sealed class PlayerBallProgress
{
    [SerializeField] private BallType ballType;
    [SerializeField] private int upgradeLevel;
    [SerializeField] private bool unlocked;

    public PlayerBallProgress(BallType ballType, int upgradeLevel, bool unlocked)
    {
        this.ballType = ballType;
        Set(upgradeLevel, unlocked);
    }

    public BallType BallType => ballType;
    public int UpgradeLevel => Mathf.Max(0, upgradeLevel);
    public bool Unlocked => unlocked;

    public void Set(int level, bool isUnlocked)
    {
        upgradeLevel = Mathf.Max(0, level);
        unlocked = isUnlocked;
    }
}
