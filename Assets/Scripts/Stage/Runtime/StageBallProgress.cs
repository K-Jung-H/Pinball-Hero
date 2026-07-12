using System;
using UnityEngine;

[Serializable]
public sealed class StageBallProgress
{
    [SerializeField] private BallType ballType = BallType.Normal;
    [SerializeField] private int upgradeLevel = 1;
    [SerializeField] private bool unlocked = true;

    public BallType BallType => ballType;
    public int UpgradeLevel => Mathf.Max(0, upgradeLevel);
    public bool Unlocked => unlocked;
}
