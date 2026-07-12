using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer characterHead;
    [SerializeField] private SpriteRenderer characterBody;

    [SerializeField] private Vector2 headLeftOffset;
    [SerializeField] private Vector2 headRightOffset;

    [SerializeField] private float headRotationOffsetZ;
    [SerializeField] private float headDownRotationLimitZ = 20f;
    [SerializeField] private float headUpRotationLimitZ = 30f;
    [SerializeField] private float flipDeadZone = 0.05f;

    private bool isFacingRight;

    public void SetAimDirection(Vector2 aimDirection)
    {
        if (aimDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        aimDirection.Normalize();

        if (aimDirection.x > flipDeadZone)
        {
            isFacingRight = true;
        }
        else if (aimDirection.x < -flipDeadZone)
        {
            isFacingRight = false;
        }

        characterHead.flipX = isFacingRight;
        characterBody.flipX = isFacingRight;

        characterHead.transform.localPosition = isFacingRight
            ? headRightOffset
            : headLeftOffset;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        float baseAngle = isFacingRight ? 0f : 180f;
        float headRotationZ = Mathf.DeltaAngle(baseAngle, angle);

        float minRotationZ = isFacingRight ? -headDownRotationLimitZ : -headUpRotationLimitZ;
        float maxRotationZ = isFacingRight ? headUpRotationLimitZ : headDownRotationLimitZ;

        headRotationZ = Mathf.Clamp(
            headRotationZ,
            minRotationZ,
            maxRotationZ
        );

        headRotationZ += isFacingRight ? headRotationOffsetZ : -headRotationOffsetZ;

        characterHead.transform.localRotation =
            Quaternion.Euler(0f, 0f, headRotationZ);
    }
}
