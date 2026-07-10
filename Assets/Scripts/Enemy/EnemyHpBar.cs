using UnityEngine;

public class EnemyHpBar : MonoBehaviour
{
    [SerializeField] private SpriteRenderer backgroundSprite;
    [SerializeField] private SpriteRenderer fillSprite;
    [SerializeField] private Gradient fillColorByHpRatio;

    private Transform fillTransform;
    private Vector2 fillFullSize;
    private Vector3 fillFullLocalPosition;
    private bool isInitialized;

    private void Awake()
    {
        CacheInitialFillTransform();
    }

    public void Initialize()
    {
        CacheInitialFillTransform();
        SetRatio(1f);
        Hide();
    }

    public void SetHp(int currentHp, int maxHp)
    {
        if (maxHp <= 0)
        {
            return;
        }

        Show();
        SetRatio((float)currentHp / maxHp);
    }

    public void Show()
    {
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void SetRatio(float value)
    {
        CacheInitialFillTransform();

        float ratio = Mathf.Clamp01(value);

        if (fillTransform != null)
        {
            Vector2 size = fillFullSize;
            size.x = fillFullSize.x * ratio;
            fillSprite.size = size;

            Vector3 position = fillFullLocalPosition;
            position.x = fillFullLocalPosition.x - fillFullSize.x * (1f - ratio) * 0.5f;
            fillTransform.localPosition = position;
        }

        if (fillSprite != null && fillColorByHpRatio != null)
        {
            fillSprite.color = fillColorByHpRatio.Evaluate(ratio);
        }
    }

    private void CacheInitialFillTransform()
    {
        if (isInitialized || fillSprite == null)
        {
            return;
        }

        fillTransform = fillSprite.transform;
        fillFullSize = fillSprite.size;
        fillFullLocalPosition = fillTransform.localPosition;
        isInitialized = true;
    }

    private void SetVisible(bool value)
    {
        if (backgroundSprite != null)
        {
            backgroundSprite.enabled = value;
        }

        if (fillSprite != null)
        {
            fillSprite.enabled = value;
        }
    }
}
