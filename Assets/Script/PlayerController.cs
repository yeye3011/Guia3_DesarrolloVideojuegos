using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;

    [Header("Cámara")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float minLookX = -80f;
    [SerializeField] private float maxLookX = 80f;

    [Header("Multitouch")]
    [SerializeField] private SplitScreenTouchZones touchZones;

    private float cameraRotationX = 0f;

    // =====================================================
    // INPUT ACTION
    // =====================================================

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.Disable();
        }
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        MovePlayer();
        LookCamera();
    }

    // =====================================================
    // MOVIMIENTO
    // =====================================================

    private void MovePlayer()
    {
        if (moveAction == null)
            return;

        Vector2 input =
            moveAction.action.ReadValue<Vector2>();

        Vector3 direction =
            transform.forward * input.y +
            transform.right * input.x;

        direction =
            Vector3.ClampMagnitude(
                direction,
                1f
            );

        transform.position +=
            direction *
            moveSpeed *
            Time.deltaTime;
    }

    // =====================================================
    // CÁMARA
    // =====================================================

    private void LookCamera()
    {
        if (touchZones == null ||
            cameraTransform == null)
        {
            return;
        }

        Vector2 look =
            touchZones.LookDelta;

        float lookX =
            look.x *
            lookSensitivity;

        float lookY =
            look.y *
            lookSensitivity;

        // Rotación horizontal de la cápsula
        transform.Rotate(
            Vector3.up * lookX
        );

        // Rotación vertical de la cámara
        cameraRotationX -= lookY;

        cameraRotationX =
            Mathf.Clamp(
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