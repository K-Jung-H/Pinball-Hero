using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillSelectionController : MonoBehaviour
{
    [SerializeField] private GameObject viewRoot;
    [SerializeField] private SkillCardPanel[] panels;
    [SerializeField] private bool enableTestOpenInput = true;
    [SerializeField] private Key testOpenKey = Key.Space;
    [SerializeField] private RunSkillInventory runSkillInventory = new RunSkillInventory();

    private readonly SkillCardResolver resolver = new SkillCardResolver();

    private SkillCardOption[] options;
    private SkillCatalogSO skillCatalog;
    private BallShooter ballShooter;
    private bool isOpen;
    private float previousTimeScale = 1f;

    public RunSkillInventory Inventory => runSkillInventory;
    public bool IsOpen => isOpen;

    public event Action SelectionCompleted;

    private void Awake()
    {
        EnsureOptionBuffer();
        Hide();
    }

    private void Update()
    {
        if (!enableTestOpenInput || isOpen || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current[testOpenKey].wasPressedThisFrame)
        {
            ShowChoices();
        }
    }

    public void Initialize(SkillCatalogSO catalog, BallShooter shooter)
    {
        skillCatalog = catalog;
        ballShooter = shooter;
    }

    public void ResetRun()
    {
        Hide();
        runSkillInventory.Clear();

        if (options != null)
        {
            Array.Clear(options, 0, options.Length);
        }
    }

    public void ShowChoices()
    {
        if (skillCatalog == null || panels == null || panels.Length == 0)
        {
            return;
        }

        EnsureOptionBuffer();
        int resolvedCount = resolver.Resolve(skillCatalog.Skills, runSkillInventory, options);

        if (resolvedCount <= 0)
        {
            Hide();
            return;
        }

        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] == null)
            {
                continue;
            }

            if (i < resolvedCount)
            {
                panels[i].Bind(options[i], OnOptionSelected);
            }
            else
            {
                panels[i].gameObject.SetActive(false);
            }
        }

        SetViewActive(true);
        PauseGame();
    }

    public void Hide()
    {
        if (panels != null)
        {
            for (int i = 0; i < panels.Length; i++)
            {
                if (panels[i] != null)
                {
                    panels[i].gameObject.SetActive(false);
                }
            }
        }

        SetViewActive(false);
        ResumeGame();
    }

    private void OnOptionSelected(SkillCardOption option)
    {
        if (!option.IsValid)
        {
            return;
        }

        bool wasNew = runSkillInventory.GetSkillLevel(option.Definition) <= 0;

        if (!runSkillInventory.AddOrUpgrade(option.Definition))
        {
            return;
        }

        ActiveBallSkillDefinitionSO activeDefinition = option.Definition as ActiveBallSkillDefinitionSO;

        if (wasNew
            && activeDefinition != null
            && activeDefinition.TargetBallType != BallType.None
            && ballShooter != null)
        {
            ballShooter.AddBall(activeDefinition.TargetBallType);
        }

        Hide();
        SelectionCompleted?.Invoke();
    }

    private void EnsureOptionBuffer()
    {
        int count = panels == null
            ? 0
            : panels.Length;

        if (options == null || options.Length != count)
        {
            options = new SkillCardOption[count];
        }
    }

    private void SetViewActive(bool active)
    {
        if (viewRoot != null)
        {
            viewRoot.SetActive(active);
        }
    }

    private void PauseGame()
    {
        if (isOpen)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isOpen = true;
    }

    private void ResumeGame()
    {
        if (!isOpen)
        {
            return;
        }

        Time.timeScale = previousTimeScale;
        isOpen = false;
    }
}
