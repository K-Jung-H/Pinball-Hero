using UnityEngine;

public class DamageTextSprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] digitRenderers;
    [SerializeField] private Sprite[] digitSprites = new Sprite[10];
    [SerializeField] private float digitGap = 0.02f;
    [SerializeField] private float fallbackDigitWidth = 0.28f;

    private int[] digitBuffer;
    private float[] digitWidthBuffer;
    private DamageTextProfile profile;
    private DamageTextSystem ownerSystem;
    private Vector3 startPosition;
    private Vector3 baseLocalScale;
    private float elapsed;
    private int activeDigitCount;
    private bool isPlaying;

    private void Awake()
    {
        baseLocalScale = transform.localScale;
        EnsureDigitBuffer();
        HideDigits();
    }

    private void Update()
    {
        if (!isPlaying || profile == null)
        {
            return;
        }

        elapsed += Time.deltaTime;

        float duration = Mathf.Max(0.0001f, profile.Duration);
        float normalizedTime = Mathf.Clamp01(elapsed / duration);

        transform.position = startPosition + Vector3.up * (profile.RiseSpeed * elapsed);
        SetAnimatedScale(profile.EvaluateScale(normalizedTime));
        SetAlpha(profile.EvaluateAlpha(normalizedTime));

        if (normalizedTime >= 1f)
        {
            Stop();
        }
    }

    public void Play(int damage, Vector3 position, DamageTextProfile textProfile, DamageTextSystem system)
    {
        EnsureDigitBuffer();

        profile = textProfile ?? DamageTextProfile.Normal;
        ownerSystem = system;
        startPosition = position;
        elapsed = 0f;
        isPlaying = true;

        transform.position = startPosition;
        SetAnimatedScale(profile.EvaluateScale(0f));

        gameObject.SetActive(true);
        SetDigits(Mathf.Max(0, damage));
        SetAlpha(profile.EvaluateAlpha(0f));
    }

    private void Stop()
    {
        isPlaying = false;
        HideDigits();

        if (ownerSystem != null)
        {
            ownerSystem.Return(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void SetAnimatedScale(float scale)
    {
        transform.localScale = new Vector3(
            baseLocalScale.x * scale,
            baseLocalScale.y * scale,
            baseLocalScale.z
        );
    }

    private void SetDigits(int value)
    {
        if (digitRenderers == null || digitRenderers.Length <= 0)
        {
            return;
        }

        int maxDigits = digitRenderers.Length;
        activeDigitCount = GetDigits(value, maxDigits);
        float totalWidth = 0f;

        for (int i = 0; i < activeDigitCount; i++)
        {
            SpriteRenderer digitRenderer = digitRenderers[i];

            if (digitRenderer == null)
            {
                continue;
            }

            int digit = digitBuffer[i];
            Sprite digitSprite = digit >= 0 && digit < digitSprites.Length
                ? digitSprites[digit]
                : null;

            digitRenderer.sprite = digitSprite;
            digitRenderer.color = profile.Color;
            digitWidthBuffer[i] = GetDigitWidth(digitSprite);
            totalWidth += digitWidthBuffer[i];
        }

        totalWidth += Mathf.Max(0, activeDigitCount - 1) * digitGap;

        float x = -totalWidth * 0.5f;

        for (int i = 0; i < digitRenderers.Length; i++)
        {
            SpriteRenderer digitRenderer = digitRenderers[i];

            if (digitRenderer == null)
            {
                continue;
            }

            bool isActive = i < activeDigitCount;
            digitRenderer.enabled = isActive;

            if (!isActive)
            {
                continue;
            }

            float digitWidth = digitWidthBuffer[i];
            Vector3 localPosition = digitRenderer.transform.localPosition;
            localPosition.x = x + digitWidth * 0.5f;
            localPosition.y = 0f;
            digitRenderer.transform.localPosition = localPosition;

            x += digitWidth + digitGap;
        }
    }

    private float GetDigitWidth(Sprite digitSprite)
    {
        if (digitSprite == null)
        {
            return fallbackDigitWidth;
        }

        float width = digitSprite.bounds.size.x;

        if (width <= 0f)
        {
            return fallbackDigitWidth;
        }

        return width;
    }

    private int GetDigits(int value, int maxDigits)
    {
        if (value <= 0)
        {
            digitBuffer[0] = 0;
            return 1;
        }

        int digitCount = 0;
        int remainingValue = value;

        while (remainingValue > 0 && digitCount < maxDigits)
        {
            digitBuffer[digitCount] = remainingValue % 10;
            remainingValue /= 10;
            digitCount++;
        }

        ReverseDigits(digitCount);
        return digitCount;
    }

    private void ReverseDigits(int digitCount)
    {
        int left = 0;
        int right = digitCount - 1;

        while (left < right)
        {
            int temp = digitBuffer[left];
            digitBuffer[left] = digitBuffer[right];
            digitBuffer[right] = temp;
            left++;
            right--;
        }
    }

    private void SetAlpha(float alpha)
    {
        for (int i = 0; i < activeDigitCount; i++)
        {
            SpriteRenderer digitRenderer = digitRenderers[i];

            if (digitRenderer == null)
            {
                continue;
            }

            Color color = digitRenderer.color;
            color.a = alpha;
            digitRenderer.color = color;
        }
    }

    private void HideDigits()
    {
        activeDigitCount = 0;

        if (digitRenderers == null)
        {
            return;
        }

        for (int i = 0; i < digitRenderers.Length; i++)
        {
            if (digitRenderers[i] != null)
            {
                digitRenderers[i].enabled = false;
            }
        }
    }

    private void EnsureDigitBuffer()
    {
        int digitCount = digitRenderers != null && digitRenderers.Length > 0
            ? digitRenderers.Length
            : 1;

        if (digitBuffer == null || digitBuffer.Length != digitCount)
        {
            digitBuffer = new int[digitCount];
        }

        if (digitWidthBuffer == null || digitWidthBuffer.Length != digitCount)
        {
            digitWidthBuffer = new float[digitCount];
        }
    }
}
