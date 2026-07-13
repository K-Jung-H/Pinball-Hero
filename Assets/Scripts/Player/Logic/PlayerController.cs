using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Health")]
    [Min(1)]
    [SerializeField] private int hpMax = 100;
    [SerializeField] private SpriteHpBar hpBar;

    [Header("Control")]
    [SerializeField] private PlayerRenderer playerRenderer;
    [SerializeField] private BallShooter ballShooter;

    private int hpCurrent;
    private Vector2 lastAimDirection = Vector2.up;

    public BallShooter BallShooter => ballShooter;
    public int HpMax => hpMax;
    public int HpCurrent => hpCurrent;
    public bool IsDead => hpCurrent <= 0;

    public event Action PlayerDied;

    private void Awake()
    {
        mainCamera = Camera.main;
        hpMax = Mathf.Max(1, hpMax);
        hpCurrent = hpMax;

        if (hpBar != null)
        {
            hpBar.Initialize(hpCurrent, hpMax);
            hpBar.Show();
        }
    }

    public void TakeDamage(int value)
    {
        if (value <= 0 || IsDead)
        {
            return;
        }

        hpCurrent = Mathf.Max(0, hpCurrent - value);

        if (hpBar != null)
        {
            hpBar.SetHp(hpCurrent, hpMax);
        }

        if (hpCurrent <= 0)
        {
            PlayerDied?.Invoke();
        }
    }

    private void Update()
    {
        UpdateAim();
    }

    private void UpdateAim()
    {
        Vector2 screenPosition;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null)
        {
            screenPosition = Mouse.current.position.ReadValue();
        }
        else
        {
            return;
        }

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        Vector2 direction = (Vector2)worldPosition - (Vector2)transform.position;

        if (direction.sqrMagnitude <= 0.0001f || direction.y <= 0f)
        {
            return;
        }

        lastAimDirection = direction.normalized;
        playerRenderer.SetAimDirection(lastAimDirection);
        ballShooter.SetLaunchDirection(lastAimDirection);
    }
}
