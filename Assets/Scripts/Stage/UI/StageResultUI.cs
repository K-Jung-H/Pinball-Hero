using TMPro;
using UnityEngine;

public sealed class StageResultUI : MonoBehaviour
{
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color failColor = Color.red;

    private void Awake()
    {
        Hide();
    }

    public void ShowSuccess()
    {
        Show("Success", successColor);
    }

    public void ShowFail()
    {
        Show("Fail", failColor);
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    private void Show(string value, Color color)
    {
        if (resultText != null)
        {
            resultText.SetText(value);
            resultText.color = color;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }
}
