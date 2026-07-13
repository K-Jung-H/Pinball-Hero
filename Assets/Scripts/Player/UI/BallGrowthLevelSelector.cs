using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BallGrowthLevelSelector : MonoBehaviour
{
    [SerializeField] private BallGrowthDefinitionSO growthDefinition;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;

    private int selectedIndex;

    public BallType BallType => growthDefinition != null
        ? growthDefinition.BallType
        : BallType.None;
    public int SelectedLevel => growthDefinition != null
        ? growthDefinition.GetLevel(selectedIndex)
        : 0;
    public bool IsConfigured => growthDefinition != null
        && growthDefinition.HasValidLevelRange
        && levelText != null
        && decreaseButton != null
        && increaseButton != null;

    private void Awake()
    {
        if (decreaseButton != null)
        {
            decreaseButton.onClick.AddListener(SelectPreviousLevel);
        }

        if (increaseButton != null)
        {
            increaseButton.onClick.AddListener(SelectNextLevel);
        }
    }

    public void Initialize(int preferredLevel)
    {
        selectedIndex = 0;

        if (growthDefinition != null
            && growthDefinition.TryGetLevelIndex(preferredLevel, out int levelIndex))
        {
            selectedIndex = levelIndex;
        }

        Refresh();
    }

    private void SelectPreviousLevel()
    {
        if (growthDefinition == null || selectedIndex <= 0)
        {
            return;
        }

        selectedIndex--;
        Refresh();
    }

    private void SelectNextLevel()
    {
        if (growthDefinition == null
            || selectedIndex >= growthDefinition.LevelCount - 1)
        {
            return;
        }

        selectedIndex++;
        Refresh();
    }

    private void Refresh()
    {
        int levelCount = growthDefinition != null
            ? growthDefinition.LevelCount
            : 0;

        if (levelText != null)
        {
            levelText.SetText("{0}", SelectedLevel);
        }

        if (decreaseButton != null)
        {
            decreaseButton.interactable = levelCount > 0 && selectedIndex > 0;
        }

        if (increaseButton != null)
        {
            increaseButton.interactable = levelCount > 0
                && selectedIndex < levelCount - 1;
        }
    }

    private void OnDestroy()
    {
        if (decreaseButton != null)
        {
            decreaseButton.onClick.RemoveListener(SelectPreviousLevel);
        }

        if (increaseButton != null)
        {
            increaseButton.onClick.RemoveListener(SelectNextLevel);
        }
    }
}
