using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Cámara")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float minLookX = -80f;
    [SerializeField] private float maxLookX = 80f;

    [SerializeField]
    private InteractionModeController modeController;

    [Header("Multitouch")]
    [SerializeField] private SplitScreenTouchZones touchZones;

    private float cameraRotationX = 0f;

    private void Update()
    {
        MovePlayer();
        LookCamera();
    }

    private void MovePlayer()
    {
        Vector2 input = touchZones.MoveAxis;

        Vector3 direction =
            transform.forward * input.y +
            transform.right * input.x;

        direction = Vector3.ClampMagnitude(direction, 1f);

        transform.position +=
            direction * moveSpeed * Time.deltaTime;
    }

    private void LookCamera()
    {
        if (modeController.IsDragMode ||
            modeController.IsSwipeMode)
        {
            return;
        }

        Vector2 look = touchZones.LookDelta;

        float lookX =
            look.x * lookSensitivity;

        float lookY =
            look.y * lookSensitivity;

        transform.Rotate(
            Vector3.up * lookX
        );

        cameraRotationX -= lookY;

        cameraRotationX = Mathf.Clamp(
            cameraRotationX,
            minLookX,
            maxLookX
        );

        cameraTransform.localRotation =
            Quaternion.Euler(
                cameraRotationX,
                0f,
                0f
            );
    }
}