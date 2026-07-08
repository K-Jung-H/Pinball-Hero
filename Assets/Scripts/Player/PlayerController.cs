using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Camera mainCamera;

    [SerializeField] private PlayerRenderer playerRenderer;

    private Vector2 lastAimDirection = Vector2.up;

    private void Awake()
    {
        mainCamera = Camera.main;
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

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        lastAimDirection = direction.normalized;
        playerRenderer.SetAimDirection(lastAimDirection);
    }
}