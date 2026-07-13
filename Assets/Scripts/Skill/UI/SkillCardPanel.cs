using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCardPanel : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private TMP_Text categoryTitleText;
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Skill Level")]
    [SerializeField] private Image[] levelImages;
    [SerializeField] private Color acquiredLevelColor;
    [SerializeField] private Color targetLevelColor;
    [SerializeField] private Color emptyLevelColor;

    private SkillCardOption option;
    private Action<SkillCardOption> selectedCallback;

    private void Awake()
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(OnClicked);
            selectButton.onClick.AddListener(OnClicked);
        }
    }

    public void Bind(SkillCardOption value, Action<SkillCardOption> onSelected)
    {
        option = value;
        selectedCallback = onSelected;

        if (!option.IsValid)
        {
            gameObject.SetActive(false);
            return;
        }

        SkillDefinitionSO definition = option.Definition;

        if (thumbnailImage != null)
        {
            thumbnailImage.sprite = definition.Icon;
            thumbnailImage.enabled = definition.Icon != null;
        }

        if (categoryTitleText != null)
        {
            categoryTitleText.text = definition.Category == SkillCategory.ActiveBall
                ? "Active"
                : "Passive";
        }

        if (skillNameText != null)
        {
            skillNameText.text = definition.DisplayName;
        }

        UpdateLevelImages();

        if (descriptionText != null)
        {
            descriptionText.text = definition.BuildDescription(option.CurrentLevel, option.TargetLevel);
        }

        gameObject.SetActive(true);
    }

    private void UpdateLevelImages()
    {
        if (levelImages == null)
        {
            return;
        }

        for (int i = 0; i < levelImages.Length; i++)
        {
            Image levelImage = levelImages[i];

            if (levelImage == null)
            {
                continue;
            }

            int level = i + 1;

            if (level <= option.CurrentLevel)
            {
                levelImage.color = acquiredLevelColor;
            }
            else if (level == option.TargetLevel)
            {
                levelImage.color = targetLevelColor;
            }
            else
            {
                levelImage.color = emptyLevelColor;
            }
        }
    }

    private void OnClicked()
    {
        if (!option.IsValid)
        {
            return;
        }

        selectedCallback?.Invoke(option);
    }
}
