using TMPro;
using UnityEngine;

public sealed class StageResultUI : MonoBehaviour
{
    [SerializeField] private GameObject viewRoot;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GameObject continueButtonRoot;

    [Header("Messages")]
    [SerializeField] private string pauseMessage = "Pause";

    [Header("Colors")]
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color failColor = Color.red;
    [SerializeField] private Color pauseColor = Color.white;

    private void Awake()
    {
        Hide();
    }

    public void ShowSuccess()
    {
        Show("Success", successColor, false);
    }

    public void ShowFail()
    {
        Show("Fail", failColor, false);
    }

    public void ShowPause()
    {
        Show(pauseMessage, pauseColor, true);
    }

    public void Hide()
    {
        if (continueButtonRoot != null)
        {
            continueButtonRoot.SetActive(false);
        }

        if (viewRoot != null)
        {
            viewRoot.SetActive(false);
        }
    }

    private void Show(string value, Color color, bool showContinueButton)
    {
        if (resultText != null)
        {
            resultText.SetText(value);
            resultText.color = color;
        }

        if (continueButtonRoot != null)
        {
            continueButtonRoot.SetActive(showContinueButton);
        }

        if (viewRoot != null)
        {
            viewRoot.SetActive(true);
        }
    }
}
