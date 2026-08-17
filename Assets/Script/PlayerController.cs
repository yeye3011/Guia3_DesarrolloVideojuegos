using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Referencias")]
    [SerializeField] private CharacterController characterController;

    // =====================================================
    // MOVIMIENTO
    // =====================================================

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    // =====================================================
    // INPUT
    // =====================================================

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;

    // =====================================================
    // CÁMARA
    // =====================================================

    [Header("Cámara")]
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private float lookSensitivity = 0.1f;

    [SerializeField] private float minLookX = -80f;

    [SerializeField] private float maxLookX = 80f;

    // =====================================================
    // MULTITOUCH
    // =====================================================

    [Header("Multitouch")]
    [SerializeField] private SplitScreenTouchZones touchZones;

    // =====================================================
    // VARIABLES
    // =====================================================

    private float cameraRotationX = 0f;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        // Si olvidamos asignarlo manualmente,
        // intentamos encontrarlo automáticamente.
        if (characterController == null)
        {
            characterController =
                GetComponent<CharacterController>();
        }

        if (characterController == null)
        {
            Debug.LogError(
                "Player necesita un Character Controller."
            );
        }
    }

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
        if (moveAction == null ||
            characterController == null)
        {
            return;
        }

        // -------------------------------------------------
        // LEER JOYSTICK
        // -------------------------------------------------

        Vector2 input =
            moveAction.action.ReadValue<Vector2>();

        // -------------------------------------------------
        // CALCULAR DIRECCIÓN
        // -------------------------------------------------

        Vector3 direction =
            transform.forward * input.y +
            transform.right * input.x;

        direction =
            Vector3.ClampMagnitude(
                direction,
                1f
            );

        // -------------------------------------------------
        // MOVER CON CHARACTER CONTROLLER
        // -------------------------------------------------

        /*
         * Antes utilizábamos:
         *
         * transform.position += ...
         *
         * Ahora CharacterController.Move se encarga
         * de respetar las colisiones con los muros.
         */

        Vector3 movement =
            direction *
            moveSpeed *
            Time.deltaTime;

        characterController.Move(
            movement
        );
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

        // -------------------------------------------------
        // ROTACIÓN HORIZONTAL DEL PLAYER
        // -------------------------------------------------

        transform.Rotate(
            Vector3.up * lookX
        );

        // -------------------------------------------------
        // ROTACIÓN VERTICAL DE LA CÁMARA
        // -------------------------------------------------

        cameraRotationX -=
            lookY;

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