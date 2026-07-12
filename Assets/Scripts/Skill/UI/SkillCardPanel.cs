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
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text descriptionText;

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
                ? "Ball"
                : "Passive";
        }

        if (skillNameText != null)
        {
            skillNameText.text = definition.DisplayName;
        }

        if (levelText != null)
        {
            levelText.text = option.IsNew
                ? "Lv.1"
                : $"Lv.{option.CurrentLevel} > Lv.{option.TargetLevel}";
        }

        if (descriptionText != null)
        {
            descriptionText.text = definition.BuildDescription(option.CurrentLevel, option.TargetLevel);
        }

        gameObject.SetActive(true);
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
